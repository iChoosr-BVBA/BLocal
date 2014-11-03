using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Models.AutomaticSynchronization;
using BLocal.Web.Manager.Models.Home;
using Newtonsoft.Json;

namespace BLocal.Web.Manager.Controllers
{
    public class AutomaticSynchronizationController : Controller
    {
        public ProviderGroupFactory ProviderGroupFactory { get; set; }

        public AutomaticSynchronizationController()
        {
            ProviderGroupFactory = new ProviderGroupFactory();
        }

        public ContentResult Index(SynchronizationSettings settings)
        {
            var leftProviders = ProviderGroupFactory.CreateProviderGroup(settings.LeftProviderGroupName);
            var rightProviders = ProviderGroupFactory.CreateProviderGroup(settings.RightProviderGroupName);

            var leftValues = leftProviders.ValueManager.GetAllValuesQualified().ToDictionary(qv => qv.Qualifier);
            var rightValues = rightProviders.ValueManager.GetAllValuesQualified().ToDictionary(qv => qv.Qualifier);
            
            var leftNotRight = leftValues.Where(lv => !rightValues.ContainsKey(lv.Key)).Select(lv => lv.Value).ToArray();
            var rightNotLeft = rightValues.Where(rv => !leftValues.ContainsKey(rv.Key)).Select(rv => rv.Value).ToArray();

            var valueDifferences = leftValues.Values
                .Join(rightValues.Values, v => v.Qualifier, v => v.Qualifier, (lv, rv) => new SynchronizationData.DoubleQualifiedValue(lv, rv))
                .Where(dv => !Equals(dv.Left.Value, dv.Right.Value))
                .ToArray();

            leftProviders.HistoryManager.AdjustHistory(leftValues.Values, settings.LeftAuthorName ?? "Automatic Synchronization");
            rightProviders.HistoryManager.AdjustHistory(rightValues.Values, settings.RightAuthorName ?? "Automatic Synchronization");

            var leftHistoryCollection = leftProviders.HistoryManager.ProvideHistory().ToDictionary(a => a.Qualifier);
            var rightHistoryCollection = rightProviders.HistoryManager.ProvideHistory().ToDictionary(a => a.Qualifier);

            var synchronizationResult = new SynchronizationResult();

            foreach (var missingRight in leftNotRight)
            {
                switch (settings.RightMissingStrategy)
                {
                    case SynchronizationSettings.MissingResolutionStrategy.History:
                        if (!rightHistoryCollection.ContainsKey(missingRight.Qualifier))
                        {
                            Create(settings.Execute, rightProviders, leftHistoryCollection, rightHistoryCollection, missingRight, synchronizationResult);
                            break;
                        }

                        var leftHistory = leftHistoryCollection[missingRight.Qualifier];
                        var rightHistory = rightHistoryCollection[missingRight.Qualifier];
                        
                        if (leftHistory.IsPreviousVersionOf(rightHistory))
                        {
                            Delete(settings.Execute, leftProviders, rightHistoryCollection, leftHistoryCollection, missingRight, synchronizationResult);
                            break;
                        }
                        if (rightHistory.IsPreviousVersionOf(leftHistory))
                        {
                            Create(settings.Execute, rightProviders, leftHistoryCollection, rightHistoryCollection, missingRight, synchronizationResult);
                            break;
                        }
                        synchronizationResult.Unresolved.Add(new Conflict(leftProviders.Name, missingRight.Qualifier.ToString()));
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.CopyNew:
                        Create(settings.Execute, rightProviders, leftHistoryCollection, rightHistoryCollection, missingRight, synchronizationResult);
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.DeleteExisting:
                        Delete(settings.Execute, leftProviders, rightHistoryCollection, leftHistoryCollection, missingRight, synchronizationResult);
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.Ignore:
                        synchronizationResult.Ignored.Add(new Ignore(leftProviders.Name, missingRight.Qualifier.ToString()));
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.ShowConflict:
                        synchronizationResult.Unresolved.Add(new Conflict(leftProviders.Name, missingRight.Qualifier.ToString()));
                        break;
                }
            }

            foreach (var missingLeft in rightNotLeft)
            {
                switch (settings.LeftMissingStrategy)
                {
                    case SynchronizationSettings.MissingResolutionStrategy.History:
                        if (!leftHistoryCollection.ContainsKey(missingLeft.Qualifier))
                        {
                            Create(settings.Execute, leftProviders, rightHistoryCollection, leftHistoryCollection, missingLeft, synchronizationResult);
                            break;
                        }

                        var rightHistory = rightHistoryCollection[missingLeft.Qualifier];
                        var leftHistory = leftHistoryCollection[missingLeft.Qualifier];

                        if (leftHistory.IsPreviousVersionOf(rightHistory))
                        {
                            Create(settings.Execute, leftProviders, rightHistoryCollection, leftHistoryCollection, missingLeft, synchronizationResult);
                            break;
                        }
                        if (rightHistory.IsPreviousVersionOf(leftHistory))
                        {
                            Delete(settings.Execute, rightProviders, leftHistoryCollection, rightHistoryCollection, missingLeft, synchronizationResult);
                            break;
                        }
                        synchronizationResult.Unresolved.Add(new Conflict(leftProviders.Name, missingLeft.Qualifier.ToString()));
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.CopyNew:
                        Create(settings.Execute, leftProviders, rightHistoryCollection, leftHistoryCollection, missingLeft, synchronizationResult);
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.DeleteExisting:
                        Delete(settings.Execute, rightProviders, leftHistoryCollection, rightHistoryCollection, missingLeft, synchronizationResult);
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.Ignore:
                        synchronizationResult.Ignored.Add(new Ignore(rightProviders.Name, missingLeft.Qualifier.ToString()));
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.ShowConflict:
                        synchronizationResult.Unresolved.Add(new Conflict(rightProviders.Name, missingLeft.Qualifier.ToString()));
                        break;
                }
            }

            foreach (var difference in valueDifferences)
            {
                switch (settings.DifferingStrategy)
                {
                    case SynchronizationSettings.DifferingResolutionStrategy.History:
                        var rightHistory = rightHistoryCollection[difference.Right.Qualifier];
                        var leftHistory = leftHistoryCollection[difference.Left.Qualifier];

                        if (leftHistory.IsPreviousVersionOf(rightHistory))
                        {
                            UpdateCreate(settings.Execute, leftProviders, rightHistoryCollection, leftHistoryCollection, difference.Right, difference.Left.Value, synchronizationResult);
                            break;
                        }
                        if (rightHistory.IsPreviousVersionOf(leftHistory))
                        {
                            UpdateCreate(settings.Execute, rightProviders, leftHistoryCollection, rightHistoryCollection, difference.Left, difference.Right.Value, synchronizationResult);
                            break;
                        }
                        synchronizationResult.Unresolved.Add(new Conflict(rightProviders.Name + " <> " + leftProviders.Name, difference.Left.Qualifier.ToString()));
                        break;

                    case SynchronizationSettings.DifferingResolutionStrategy.UseLeft:
                        UpdateCreate(settings.Execute, rightProviders, leftHistoryCollection, rightHistoryCollection, difference.Left, difference.Right.Value, synchronizationResult);
                        break;
                    case SynchronizationSettings.DifferingResolutionStrategy.UseRight:
                        UpdateCreate(settings.Execute, leftProviders, rightHistoryCollection, leftHistoryCollection, difference.Right, difference.Left.Value, synchronizationResult);
                        break;
                    case SynchronizationSettings.DifferingResolutionStrategy.Ignore:
                        synchronizationResult.Ignored.Add(new Ignore(rightProviders.Name + " <> " + leftProviders.Name, difference.Left.Qualifier.ToString()));
                        break;
                    case SynchronizationSettings.DifferingResolutionStrategy.ShowConflict:
                        synchronizationResult.Unresolved.Add(new Conflict(rightProviders.Name + " <> " + leftProviders.Name, difference.Left.Qualifier.ToString()));
                        break;
                }
            }

            if (settings.Execute)
            {
                foreach (var audit in leftHistoryCollection.Where(audit => !rightHistoryCollection.ContainsKey(audit.Key)))
                    rightHistoryCollection.Add(audit.Key, audit.Value);
                foreach (var audit in rightHistoryCollection.Where(audit => !leftHistoryCollection.ContainsKey(audit.Key)))
                    leftHistoryCollection.Add(audit.Key, audit.Value);

                leftProviders.HistoryManager.RewriteHistory(leftHistoryCollection.Values);
                rightProviders.HistoryManager.RewriteHistory(rightHistoryCollection.Values);

                leftProviders.ValueManager.Persist();
                rightProviders.ValueManager.Persist();

                if(leftProviders.ValueManager != leftProviders.HistoryManager)
                    leftProviders.HistoryManager.Persist();

                if(rightProviders.ValueManager != rightProviders.HistoryManager)
                    rightProviders.HistoryManager.Persist();
            }

            return Content(JsonConvert.SerializeObject(synchronizationResult), "application/json", Encoding.UTF8);
        }

        private void UpdateCreate(bool execute, ProviderGroup targetProviderGroup, Dictionary<Qualifier.Unique, QualifiedHistory> sourceHistoryCollection, Dictionary<Qualifier.Unique, QualifiedHistory> targetHistoryCollection, QualifiedValue sourceQualifiedValue, String targetOldValue, SynchronizationResult result)
        {
            if (execute)
            {
                if (sourceHistoryCollection.ContainsKey(sourceQualifiedValue.Qualifier))
                    targetHistoryCollection[sourceQualifiedValue.Qualifier] = sourceHistoryCollection[sourceQualifiedValue.Qualifier];

                targetProviderGroup.ValueManager.UpdateCreateValue(new QualifiedValue(sourceQualifiedValue.Qualifier, sourceQualifiedValue.Value));
            }
            result.Updated.Add(new Update(targetProviderGroup.Name, sourceQualifiedValue.Qualifier.ToString(), targetOldValue, sourceQualifiedValue.Value));
        }

        private void Create(bool execute, ProviderGroup targetProviderGroup, Dictionary<Qualifier.Unique, QualifiedHistory> sourceHistoryCollection, Dictionary<Qualifier.Unique, QualifiedHistory> targetHistoryCollection, QualifiedValue sourceQualifiedValue, SynchronizationResult result)
        {
            if (execute)
            {
                if (sourceHistoryCollection.ContainsKey(sourceQualifiedValue.Qualifier))
                    targetHistoryCollection[sourceQualifiedValue.Qualifier] = sourceHistoryCollection[sourceQualifiedValue.Qualifier];

                targetProviderGroup.ValueManager.CreateValue(sourceQualifiedValue.Qualifier, sourceQualifiedValue.Value);
            }
            result.Created.Add(new Creation(targetProviderGroup.Name, sourceQualifiedValue.Qualifier.ToString(), sourceQualifiedValue.Value));
        }

        private void Delete(bool execute, ProviderGroup targetProviderGroup, Dictionary<Qualifier.Unique, QualifiedHistory> sourceHistoryCollection, Dictionary<Qualifier.Unique, QualifiedHistory> targetHistoryCollection, QualifiedValue targetQualifiedValue, SynchronizationResult result)
        {

            if (execute)
            {
                if (sourceHistoryCollection.ContainsKey(targetQualifiedValue.Qualifier))
                    targetHistoryCollection[targetQualifiedValue.Qualifier] = sourceHistoryCollection[targetQualifiedValue.Qualifier];

                targetProviderGroup.ValueManager.DeleteValue(targetQualifiedValue.Qualifier);
            }
            result.Removed.Add(new Removal(targetProviderGroup.Name, targetQualifiedValue.Qualifier.ToString(), targetQualifiedValue.Value));
        }
    }
}
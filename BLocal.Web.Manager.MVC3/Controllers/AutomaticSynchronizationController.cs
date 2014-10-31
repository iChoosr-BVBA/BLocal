using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using BLocal.Core;
using BLocal.Web.Manager.Business;
using BLocal.Web.Manager.Models;
using BLocal.Web.Manager.Models.AutomaticSynchronization;
using Newtonsoft.Json;

namespace BLocal.Web.Manager.Controllers
{
    public class AutomaticSynchronizationController : Controller
    {
        public ProviderPairFactory ProviderPairFactory { get; set; }

        public AutomaticSynchronizationController()
        {
            ProviderPairFactory = new ProviderPairFactory();
        }

        public ContentResult Index(SynchronizationSettings settings)
        {
            var leftPair = ProviderPairFactory.CreateProviderPair(settings.LeftProviderPairName);
            var rightPair = ProviderPairFactory.CreateProviderPair(settings.RightProviderPairName);

            leftPair.ValueManager.Reload();
            rightPair.ValueManager.Reload();

            var leftValues = leftPair.ValueManager.GetAllValuesQualified().ToDictionary(qv => qv.Qualifier);
            var rightValues = rightPair.ValueManager.GetAllValuesQualified().ToDictionary(qv => qv.Qualifier);
            
            var leftNotRight = leftValues.Where(lv => !rightValues.ContainsKey(lv.Key)).Select(lv => lv.Value).ToArray();
            var rightNotLeft = rightValues.Where(rv => !leftValues.ContainsKey(rv.Key)).Select(rv => rv.Value).ToArray();

            var valueDifferences = leftValues.Values
                .Join(rightValues.Values, v => v.Qualifier, v => v.Qualifier, (lv, rv) => new SynchronizationData.DoubleQualifiedValue(lv, rv))
                .Where(dv => !Equals(dv.Left.Value, dv.Right.Value))
                .ToArray();

            var leftAudits = leftPair.ValueManager.GetAudits().ToDictionary(a => a.Qualifier);
            var rightAudits = rightPair.ValueManager.GetAudits().ToDictionary(a => a.Qualifier);

            var synchronizationResult = new SynchronizationResult();

            foreach (var missingRight in leftNotRight)
            {
                switch (settings.RightMissingStrategy)
                {
                    case SynchronizationSettings.MissingResolutionStrategy.Audit:
                            if (!rightAudits.ContainsKey(missingRight.Qualifier))
                            {
                                Create(settings.Execute, rightPair, leftAudits, rightAudits, missingRight, synchronizationResult);
                                break;
                            }

                            var rightAudit = rightAudits[missingRight.Qualifier];
                            var leftAudit = leftAudits[missingRight.Qualifier];
                            if (leftAudit.LatestValueHash == rightAudit.PreviousValueHash && leftAudit.PreviousValueHash == rightAudit.LatestValueHash)
                            {
                                if(leftAudit.LatestUpdate > rightAudit.LatestUpdate)
                                    Create(settings.Execute, rightPair, leftAudits, rightAudits, missingRight, synchronizationResult);
                                else if (leftAudit.LatestUpdate < rightAudit.LatestUpdate)
                                    Delete(settings.Execute, leftPair, rightAudits, leftAudits, missingRight, synchronizationResult);
                                else
                                    synchronizationResult.Unresolved.Add(new Conflict(leftPair.Name, missingRight.Qualifier.ToString()));
                                break;
                            }
                            if (rightAudit.LatestValueHash == leftAudit.PreviousValueHash)
                            {
                                Create(settings.Execute, rightPair, leftAudits, rightAudits, missingRight, synchronizationResult);
                                break;
                            }
                            if (leftAudit.LatestValueHash == rightAudit.PreviousValueHash)
                            {
                                Delete(settings.Execute, leftPair, rightAudits, leftAudits, missingRight, synchronizationResult);
                                break;
                            }
                            synchronizationResult.Unresolved.Add(new Conflict(leftPair.Name, missingRight.Qualifier.ToString()));
                            break;
                        case SynchronizationSettings.MissingResolutionStrategy.CopyNew:
                            Create(settings.Execute, rightPair, leftAudits, rightAudits, missingRight, synchronizationResult);
                            break;
                        case SynchronizationSettings.MissingResolutionStrategy.DeleteExisting:
                            Delete(settings.Execute, leftPair, rightAudits, leftAudits, missingRight, synchronizationResult);
                            break;
                        case SynchronizationSettings.MissingResolutionStrategy.Ignore:
                            synchronizationResult.Ignored.Add(new Ignore(leftPair.Name, missingRight.Qualifier.ToString()));
                            break;
                        case SynchronizationSettings.MissingResolutionStrategy.ShowConflict:
                            synchronizationResult.Unresolved.Add(new Conflict(leftPair.Name, missingRight.Qualifier.ToString()));
                            break;
                }
            }

            foreach (var missingLeft in rightNotLeft)
            {
                switch (settings.LeftMissingStrategy)
                {
                    case SynchronizationSettings.MissingResolutionStrategy.Audit:
                        if (!leftAudits.ContainsKey(missingLeft.Qualifier))
                        {
                            Create(settings.Execute, leftPair, rightAudits, leftAudits, missingLeft, synchronizationResult);
                            break;
                        }

                        var rightAudit = rightAudits[missingLeft.Qualifier];
                        var leftAudit = leftAudits[missingLeft.Qualifier];
                        if (leftAudit.LatestValueHash == rightAudit.PreviousValueHash && leftAudit.PreviousValueHash == rightAudit.LatestValueHash)
                        {
                            if (leftAudit.LatestUpdate > rightAudit.LatestUpdate)
                                Delete(settings.Execute, rightPair, leftAudits, rightAudits, missingLeft, synchronizationResult);
                            else if (leftAudit.LatestUpdate < rightAudit.LatestUpdate)
                                Create(settings.Execute, leftPair, rightAudits, leftAudits, missingLeft, synchronizationResult);
                            else
                                synchronizationResult.Unresolved.Add(new Conflict(leftPair.Name, missingLeft.Qualifier.ToString()));
                            break;
                        }
                        if (leftAudit.LatestValueHash == rightAudit.PreviousValueHash)
                        {
                            Create(settings.Execute, leftPair, rightAudits, leftAudits, missingLeft, synchronizationResult);
                            break;
                        }
                        if (rightAudit.LatestValueHash == leftAudit.PreviousValueHash)
                        {
                            Delete(settings.Execute, rightPair, leftAudits, rightAudits, missingLeft, synchronizationResult);
                            break;
                        }
                        synchronizationResult.Unresolved.Add(new Conflict(leftPair.Name, missingLeft.Qualifier.ToString()));
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.CopyNew:
                        Create(settings.Execute, leftPair, rightAudits, leftAudits, missingLeft, synchronizationResult);
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.DeleteExisting:
                        Delete(settings.Execute, rightPair, leftAudits, rightAudits, missingLeft, synchronizationResult);
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.Ignore:
                        synchronizationResult.Ignored.Add(new Ignore(rightPair.Name, missingLeft.Qualifier.ToString()));
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.ShowConflict:
                        synchronizationResult.Unresolved.Add(new Conflict(rightPair.Name, missingLeft.Qualifier.ToString()));
                        break;
                }
            }

            foreach (var difference in valueDifferences)
            {
                switch (settings.DifferingStrategy)
                {
                    case SynchronizationSettings.DifferingResolutionStrategy.Audit:
                        var rightAudit = rightAudits[difference.Right.Qualifier];
                        var leftAudit = leftAudits[difference.Left.Qualifier];
                        if (leftAudit.LatestValueHash == rightAudit.PreviousValueHash && leftAudit.PreviousValueHash == rightAudit.LatestValueHash)
                        {
                            if (leftAudit.LatestUpdate > rightAudit.LatestUpdate)
                                UpdateCreate(settings.Execute, rightPair, leftAudits, rightAudits, difference.Left, difference.Right.Value, synchronizationResult);
                            else if (leftAudit.LatestUpdate < rightAudit.LatestUpdate)
                                UpdateCreate(settings.Execute, leftPair, rightAudits, leftAudits, difference.Right, difference.Left.Value, synchronizationResult);
                            else
                                synchronizationResult.Unresolved.Add(new Conflict(rightPair.Name + " <> " + leftPair.Name, difference.Left.Qualifier.ToString()));
                            break;
                        }
                        if (leftAudit.LatestValueHash == rightAudit.PreviousValueHash)
                        {
                            UpdateCreate(settings.Execute, leftPair, rightAudits, leftAudits, difference.Right, difference.Left.Value, synchronizationResult);
                            break;
                        }
                        if (rightAudit.LatestValueHash == leftAudit.PreviousValueHash)
                        {
                            UpdateCreate(settings.Execute, rightPair, leftAudits, rightAudits, difference.Left, difference.Right.Value, synchronizationResult);
                            break;
                        }
                        synchronizationResult.Unresolved.Add(new Conflict(rightPair.Name + " <> " + leftPair.Name, difference.Left.Qualifier.ToString()));
                        break;

                    case SynchronizationSettings.DifferingResolutionStrategy.UseLeft:
                        UpdateCreate(settings.Execute, rightPair, leftAudits, rightAudits, difference.Left, difference.Right.Value, synchronizationResult);
                        break;
                    case SynchronizationSettings.DifferingResolutionStrategy.UseRight:
                        UpdateCreate(settings.Execute, leftPair, rightAudits, leftAudits, difference.Right, difference.Left.Value, synchronizationResult);
                        break;
                    case SynchronizationSettings.DifferingResolutionStrategy.Ignore:
                        synchronizationResult.Ignored.Add(new Ignore(rightPair.Name + " <> " + leftPair.Name, difference.Left.Qualifier.ToString()));
                        break;
                    case SynchronizationSettings.DifferingResolutionStrategy.ShowConflict:
                        synchronizationResult.Unresolved.Add(new Conflict(rightPair.Name + " <> " + leftPair.Name, difference.Left.Qualifier.ToString()));
                        break;
                }
            }

            if (settings.Execute)
            {
                foreach (var audit in leftAudits.Where(audit => !rightAudits.ContainsKey(audit.Key)))
                    rightAudits.Add(audit.Key, audit.Value);
                foreach (var audit in rightAudits.Where(audit => !leftAudits.ContainsKey(audit.Key)))
                    leftAudits.Add(audit.Key, audit.Value);

                leftPair.ValueManager.SetAudits(leftAudits.Values);
                rightPair.ValueManager.SetAudits(rightAudits.Values);

                leftPair.ValueManager.Persist();
                rightPair.ValueManager.Persist();
            }

            return Content(JsonConvert.SerializeObject(synchronizationResult), "application/json", Encoding.UTF8);
        }

        private void UpdateCreate(bool execute, ProviderPair targetPair, Dictionary<Qualifier.Unique, LocalizationAudit> sourceAudits, Dictionary<Qualifier.Unique, LocalizationAudit> targetAudits, QualifiedValue sourceQualifiedValue, String targetOldValue, SynchronizationResult result)
        {
            if (execute)
            {
                if (sourceAudits.ContainsKey(sourceQualifiedValue.Qualifier))
                    targetAudits[sourceQualifiedValue.Qualifier] = sourceAudits[sourceQualifiedValue.Qualifier];

                targetPair.ValueManager.UpdateCreateValue(new QualifiedValue(sourceQualifiedValue.Qualifier, sourceQualifiedValue.Value));
            }
            result.Updated.Add(new Update(targetPair.Name, sourceQualifiedValue.Qualifier.ToString(), targetOldValue, sourceQualifiedValue.Value));
        }

        private void Create(bool execute, ProviderPair targetPair, Dictionary<Qualifier.Unique, LocalizationAudit> sourceAudits, Dictionary<Qualifier.Unique, LocalizationAudit> targetAudits, QualifiedValue sourceQualifiedValue, SynchronizationResult result)
        {
            if (execute)
            {
                if(sourceAudits.ContainsKey(sourceQualifiedValue.Qualifier))
                    targetAudits[sourceQualifiedValue.Qualifier] = sourceAudits[sourceQualifiedValue.Qualifier];

                targetPair.ValueManager.CreateValue(sourceQualifiedValue.Qualifier, sourceQualifiedValue.Value);
            }
            result.Created.Add(new Creation(targetPair.Name, sourceQualifiedValue.Qualifier.ToString(), sourceQualifiedValue.Value));
        }

        private void Delete(bool execute, ProviderPair targetPair, Dictionary<Qualifier.Unique, LocalizationAudit> sourceAudits, Dictionary<Qualifier.Unique, LocalizationAudit> targetAudits, QualifiedValue targetQualifiedValue, SynchronizationResult result)
        {

            if (execute)
            {
                if (sourceAudits.ContainsKey(targetQualifiedValue.Qualifier))
                    targetAudits[targetQualifiedValue.Qualifier] = sourceAudits[targetQualifiedValue.Qualifier];

                targetPair.ValueManager.DeleteValue(targetQualifiedValue.Qualifier);
            }
            result.Removed.Add(new Removal(targetPair.Name, targetQualifiedValue.Qualifier.ToString(), targetQualifiedValue.Value));
        }

        public class SynchronizationResult
        {
            public readonly List<Creation> Created = new List<Creation>();
            public readonly List<Removal> Removed = new List<Removal>();
            public readonly List<Update> Updated = new List<Update>();
            public readonly List<Ignore> Ignored = new List<Ignore>();
            public readonly List<Conflict> Unresolved = new List<Conflict>();
        }

        public class Update
        {
            public String ProviderPairName { get; set; }
            public String Qualifier { get; set; }
            public String OldValue { get; set; }
            public String NewValue { get; set; }

            public Update(String providerPairName, String qualifier, String oldValue, String newValue)
            {
                ProviderPairName = providerPairName;
                Qualifier = qualifier;
                OldValue = oldValue;
                NewValue = newValue;
            }
        }

        public class Ignore
        {
            public String ProviderPairName { get; set; }
            public String Qualifier { get; set; }

            public Ignore(String providerPairName, String qualifier)
            {
                ProviderPairName = providerPairName;
                Qualifier = qualifier;
            }
        }

        public class Conflict
        {
            public String ProviderPairName { get; set; }
            public String Qualifier { get; set; }

            public Conflict(String providerPairName, String qualifier)
            {
                ProviderPairName = providerPairName;
                Qualifier = qualifier;
            }
        }

        public class Creation
        {
            public String ProviderPairName { get; set; }
            public String Qualifier { get; set; }
            public String NewValue { get; set; }

            public Creation(String providerPairName, String qualifier, String newValue)
            {
                ProviderPairName = providerPairName;
                Qualifier = qualifier;
                NewValue = newValue;
            }
        }
        public class Removal
        {
            public String ProviderPairName { get; set; }
            public String Qualifier { get; set; }
            public String OldValue { get; set; }

            public Removal(String providerPairName, String qualifier, String oldValue)
            {
                ProviderPairName = providerPairName;
                Qualifier = qualifier;
                OldValue = oldValue;
            }
        }
    }
}
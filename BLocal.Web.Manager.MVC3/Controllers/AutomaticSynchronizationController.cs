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

            var leftValues = leftPair.ValueManager.GetAllValuesQualified().ToDictionary(qv => qv.Qualifier);
            var rightValues = rightPair.ValueManager.GetAllValuesQualified().ToDictionary(qv => qv.Qualifier);
            
            var leftNotRight = leftValues.Where(lv => !rightValues.ContainsKey(lv.Key)).Select(lv => lv.Value).ToArray();
            var rightNotLeft = rightValues.Where(rv => !leftValues.ContainsKey(rv.Key)).Select(rv => rv.Value).ToArray();

            var valueDifferences = leftValues.Values
                .Join(rightValues.Values, v => v.Qualifier, v => v.Qualifier, (lv, rv) => new SynchronizationData.DoubleQualifiedValue(lv, rv))
                .Where(dv => !Equals(dv.Left.Value, dv.Right.Value))
                .ToArray();

            var synchronizationResult = new SynchronizationResult();

            foreach (var missingRight in leftNotRight)
            {
                switch (settings.RightMissingStrategy)
                {
                        case SynchronizationSettings.MissingResolutionStrategy.CopyNew:
                            if (settings.Execute)
                                rightPair.ValueManager.CreateValue(missingRight.Qualifier, missingRight.Value);
                            synchronizationResult.Created.Add(new Creation(rightPair.Name, missingRight.Qualifier.ToString(), missingRight.Value));
                            break;
                        case SynchronizationSettings.MissingResolutionStrategy.DeleteExisting:
                            if (settings.Execute)
                                leftPair.ValueManager.DeleteValue(missingRight.Qualifier);
                            synchronizationResult.Removed.Add(new Removal(leftPair.Name, missingRight.Qualifier.ToString(), missingRight.Value));
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
                    case SynchronizationSettings.MissingResolutionStrategy.CopyNew:
                        if (settings.Execute)
                            leftPair.ValueManager.CreateValue(missingLeft.Qualifier, missingLeft.Value);
                        synchronizationResult.Created.Add(new Creation(leftPair.Name, missingLeft.Qualifier.ToString(), missingLeft.Value));
                        break;
                    case SynchronizationSettings.MissingResolutionStrategy.DeleteExisting:
                        if (settings.Execute)
                            rightPair.ValueManager.DeleteValue(missingLeft.Qualifier);
                        synchronizationResult.Removed.Add(new Removal(rightPair.Name, missingLeft.Qualifier.ToString(), missingLeft.Value));
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
                    case SynchronizationSettings.DifferingResolutionStrategy.UseLeft:
                        if (settings.Execute)
                            rightPair.ValueManager.UpdateCreateValue(new QualifiedValue(difference.Left.Qualifier, difference.Left.Value));
                        synchronizationResult.Updated.Add(new Update(rightPair.Name, difference.Left.Qualifier.ToString(), difference.Right.Value, difference.Left.Value));
                        break;
                    case SynchronizationSettings.DifferingResolutionStrategy.UseRight:
                        if (settings.Execute)
                            leftPair.ValueManager.UpdateCreateValue(new QualifiedValue(difference.Right.Qualifier, difference.Right.Value));
                        synchronizationResult.Updated.Add(new Update(leftPair.Name, difference.Right.Qualifier.ToString(), difference.Left.Value, difference.Right.Value));
                        break;
                    case SynchronizationSettings.DifferingResolutionStrategy.Ignore:
                        synchronizationResult.Ignored.Add(new Ignore(rightPair.Name + " <> " + leftPair.Name, difference.Left.Qualifier.ToString()));
                        break;
                    case SynchronizationSettings.DifferingResolutionStrategy.ShowConflict:
                        synchronizationResult.Unresolved.Add(new Conflict(rightPair.Name + " <> " + leftPair.Name, difference.Left.Qualifier.ToString()));
                        break;
                }
            }

            return Content(JsonConvert.SerializeObject(synchronizationResult), "application/json", Encoding.UTF8);
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
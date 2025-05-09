// Copyright © 2020 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections;
using Tobii.XR;
using UnityEngine;
using UnityEngine.UI;

namespace Tobii.Ocumen.Filters.Samples
{
    /// <summary>
    /// Populates the info text fields with eye tracking metadata and time sync data.
    /// </summary>
    public class GeneralInfoManager : MonoBehaviour
    {
#pragma warning disable 649 // Field is never assigned
        [SerializeField] private Text metaDataContent;
        [SerializeField] private Text timeSyncContent;
#pragma warning restore 649

        private void Start()
        {
            UpdateMetaDataContent();
            StartCoroutine(TimesyncCoroutine());
        }

        /// <summary>
        /// Populate the metadata content text field. 
        /// </summary>
        private void UpdateMetaDataContent()
        {
            var metaData = TobiiXR.Advanced.GetMetadata();
            metaDataContent.text = $"{metaData.Model}\n{metaData.RuntimeVersion}\n{metaData.SerialNumber}\n{metaData.OutputFrequency}";
        }

        /// <summary>
        /// Perform a time sync operation every 2 seconds and then populates the time sync content text with the data created.
        /// </summary>
        /// <returns></returns>
        private IEnumerator TimesyncCoroutine()
        {
            while (true)
            {
                var handle = TobiiXR.Advanced.StartTimesyncJob();
                while (!handle.IsCompleted)
                {
                    yield return new WaitForEndOfFrame();
                }

                var data = TobiiXR.Advanced.FinishTimesyncJob();
                UpdateTimeSyncContent(data);

                yield return new WaitForSecondsRealtime(2f);
            }
        }

        /// <summary>
        /// Populate the time sync content text field.
        /// </summary>
        /// <param name="data"></param>
        private void UpdateTimeSyncContent(TobiiXR_AdvancedTimesyncData? data)
        {
            if (!data.HasValue) return;

            var d = data.Value;
            timeSyncContent.text = string.Format("{0}\n{1}\n{2}\n{3}", d.StartSystemTimestamp + " µs",
                d.EndSystemTimestamp + " µs",
                (d.EndSystemTimestamp - d.StartSystemTimestamp) + " µs", d.DeviceTimestamp + " µs");
        }
    }
}
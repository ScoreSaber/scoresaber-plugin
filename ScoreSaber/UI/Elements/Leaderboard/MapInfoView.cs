﻿using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Extensions;
using System.Threading;
using ScoreSaber.UI.Leaderboard;
using BeatSaberMarkupLanguage;
using UnityEngine;
using IPA.Utilities.Async;

namespace ScoreSaber.UI.Elements.Leaderboard {
    internal class MapInfoView {
        #region BSML Components
        [UIComponent("map-info-modal-root")]
        public ModalView detailModalRoot = null;
        [UIComponent("map-name-text")]
        protected CurvedTextMeshPro _mapNameText = null;
        [UIComponent("map-info-top")]
        protected ImageView _mapInfoTop = null;
        [UIComponent("map-info-picture")]
        protected ImageView _mapInfoPicture = null;
        [UIComponent("map-author-text")]
        protected CurvedTextMeshPro _mapAuthorText = null;
        [UIComponent("map-upload-date-text")]
        protected CurvedTextMeshPro _mapUploadDateText = null;
        [UIComponent("map-plays-text")]
        protected CurvedTextMeshPro _mapPlaysText = null;
        [UIComponent("map-status-date-text")]
        protected CurvedTextMeshPro _mapStatusDateText = null;
        [UIComponent("map-info-line-border")]
        protected ImageView _mapInfoLineBorder = null;

        #endregion

        internal LeaderboardInfo _currentMapInfo { get; set; }
        internal BeatmapLevel _currentMap { get; set; }

        [UIAction("#post-parse")]
        public void Parsed() {
            _mapInfoTop.material = Utilities.ImageResources.NoGlowMat;

            var modalPic = _mapInfoPicture;
            PanelView.ImageSkew(ref modalPic) = 0f;
            PanelView.ImageSkew(ref _mapInfoLineBorder) = 0f;
            PanelView.ImageSkew(ref _mapInfoTop) = 0f;

            modalPic.material = Plugin.NoGlowMatRound;
        }

        [UIObject("map-info-set")]
        internal GameObject mapInfoSet = null;

        [UIObject("map-info-set-loading")]
        internal GameObject mapInfoSetLoading = null;

        [UIAction("map-info-url-click")]
        public void MapInfoUrlClick() {
            if (_currentMapInfo == null) return;
            Application.OpenURL($"https://scoresaber.com/leaderboard/{_currentMapInfo.id}");
        }

        public void ResetName() {
            _mapNameText.text = "Loading...";
        }

        public async void SetImage(BeatmapLevel level) {
            _mapInfoPicture.sprite = await level.previewMediaData.GetCoverSpriteAsync();
        }

        internal string GetMapStatusString() {
            bool isRanked = _currentMapInfo.ranked;
            bool isQualified = _currentMapInfo.qualified;
            bool isLoved = _currentMapInfo.loved;

            if (isRanked)
                return $"Ranked {_currentMapInfo.stars}<size=70%>★</size> (<size=75%><color=#a6a6a6>{(_currentMapInfo.rankedDate.HasValue ? _currentMapInfo.rankedDate.Value.ToString("dd/MM/yy") : string.Empty)}</color></size>)";
            else if (isQualified)
                return $"Qualified - (<size=75%><color=#a6a6a6>{(_currentMapInfo.qualifiedDate.HasValue ? _currentMapInfo.qualifiedDate.Value.ToString("dd/MM/yy") : string.Empty)}</color></size>)";
            else if (isLoved)
                return $"Loved - (<size=75%><color=#a6a6a6>{(_currentMapInfo.lovedDate.HasValue ? _currentMapInfo.lovedDate.Value.ToString("dd/MM/yy") : string.Empty)}</color></size>)";
            else
                return $"Unranked";
        }

        public void SetScoreInfo(LeaderboardInfo mapInfo) {
            if (mapInfo == null || _currentMap == null) return;
            try {
                _currentMapInfo = mapInfo;
                _mapNameText.text = $"{mapInfo.songName}";
                UnityMainThreadTaskScheduler.Factory.StartNew(() => SetImage(_currentMap));
                _mapAuthorText.text = _currentMap.hasPrecalculatedData ? "Mapped by Beat Games" : "Mapped by " + $"{mapInfo.levelAuthorName}";
                _mapUploadDateText.SetFancyText("Uploaded", $"{mapInfo.createdDate:dd/MM/yy}");
                _mapPlaysText.SetFancyText("Plays", $"{mapInfo.plays} ({mapInfo.dailyPlays} Last 24h)");
                _mapStatusDateText.SetFancyText("Status", $"{GetMapStatusString()}");
                mapInfoSetLoading.gameObject.SetActive(false);
                mapInfoSet.SetActive(true);
            } catch (Exception e) {
                mapInfoSetLoading.gameObject.SetActive(true);
                mapInfoSet.SetActive(false);
                Plugin.Log.Error(e);
            }
        }

    }
}
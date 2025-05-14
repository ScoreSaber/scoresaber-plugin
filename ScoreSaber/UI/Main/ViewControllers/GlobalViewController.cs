using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Services;
using ScoreSaber.Extensions;
using ScoreSaber.UI.Elements.Profile;
using ScoreSaber.UI.Leaderboard;
using ScoreSaber.UI.Other;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static ScoreSaber.Core.Services.GlobalLeaderboardService;

namespace ScoreSaber.UI.Main.ViewControllers
{
    [HotReload]
    internal class GlobalViewController : BSMLAutomaticViewController
    {

        #region UI Properties

        [UIParams]
        protected readonly BSMLParserParams _parserParams = null;

        [UIComponent("leaderboard")]
        protected readonly CustomCellListTableData _leaderboard = null;

        [UIComponent("up-button")]
        protected readonly Button _upButton = null;

        [UIComponent("down-button")]
        protected readonly Button _downButton = null;

        [UIComponent("rank-text")]
        protected readonly TextMeshProUGUI _rankText = null;

        [UIComponent("profile-modal")]
        protected readonly ProfileDetailView _profileDetailView = null;

        [UIComponent("dismiss-button")]
        protected readonly Button _dismissButton = null;

        [UIComponent("more-info-button")]
        protected readonly Button _moreInfoButton = null;

        [UIValue("current-rank-cells")]
        protected readonly List<object> _rankCells = new List<object>();

        [UIValue("global-loading")]
        protected bool _globalLoading => !globalSet;

        private bool _globalSet;
        [UIValue("global-set")]
        protected bool globalSet {
            get => _globalSet;
            set {
                _globalSet = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(_globalLoading));
            }
        }

        [UIComponent("global-scope")]
        protected readonly ClickableImage _globalScopeImage = null;
        [UIComponent("player-scope")]
        protected readonly ClickableImage _playerScopeImage = null;
        [UIComponent("friends-scope")]
        protected readonly ClickableImage _friendsScopeImage = null;
        [UIComponent("location-scope")]
        protected readonly ClickableImage _countryScopeImage = null;

        private Color _selectedColor = new Color(0.60f, 0.80f, 1);
        #endregion

        #region Handlers
        [UIAction("global-up")] private void GlobalUpClicked() => PageButtonClicked(false);
        [UIAction("global-down")] private void GlobalDownClicked() => PageButtonClicked(true);
        [UIAction("global-click")] private void GlobalTextClicked() => Application.OpenURL("https://scoresaber.com/global");

        [UIAction("global-scope-click")] private void GlobalScopeClicked() => ScopeClicked(GlobalPlayerScope.Global);
        [UIAction("player-scope-click")] private void PlayerScopeClicked() => ScopeClicked(GlobalPlayerScope.AroundPlayer);
        [UIAction("friends-scope-click")] private void FriendsScopeClicked() => ScopeClicked(GlobalPlayerScope.Friends);
        
        [UIAction("location-scope-click")]
        private void LocationScopeClicked() {
            if(Plugin.Settings.locationFilterMode.ToLower() == "country") {
                ScopeClicked(GlobalPlayerScope.Country);
            }
            else if(Plugin.Settings.locationFilterMode.ToLower() == "region") {
                ScopeClicked(GlobalPlayerScope.Region);
            }
            else {
                Plugin.Log.Error("Invalid location filter mode, falling back to country");
                ScopeClicked(GlobalPlayerScope.Country);
            }
        }
        [UIAction("more-info-click")] private void MoreInfoClicked() => Application.OpenURL("https://wiki.scoresaber.com/ranking-system.html");
        #endregion

        private int _page = 1;
        private GlobalPlayerScope _scope;
        private string _requestId = string.Empty;
        private DiContainer _container = null;
        private GlobalLeaderboardService _globalLeaderboardService = null;

        private readonly List<GlobalCell> _globalCells = new List<GlobalCell>();

        [Inject]
        protected void Construct(DiContainer container, GlobalLeaderboardService globalLeaderboardService) {

            _container = container;
            _globalLeaderboardService = globalLeaderboardService;
            Plugin.Log.Debug("GlobalViewController Setup");
        }

        [UIAction("#post-parse")]
        public void Parsed() {

            _upButton.transform.localScale *= .7f;
            _downButton.transform.localScale *= .7f;
            _globalScopeImage.color = _selectedColor;
            _globalScopeImage.DefaultColor = _selectedColor;


            Button[] buttons = new Button[2] { _dismissButton, _moreInfoButton };
            foreach (var button in buttons) {
                foreach (var imageView in button.GetComponentsInChildren<ImageView>()) {
                    var image = imageView;
                    PanelView.ImageSkew(ref image) = 0f;
                }
            }

            _container.Inject(_profileDetailView);
            RefreshDelayed().RunTask();
        }

        private void ScopeClicked(GlobalPlayerScope scope) {

            if (_scope == scope) { return; }
            _scope = scope;

            if (scope == GlobalPlayerScope.AroundPlayer) {
                _upButton.interactable = false;
                _downButton.interactable = false;
            } else {
                _upButton.interactable = true;
                _downButton.interactable = true;
            }

            _globalScopeImage.DefaultColor = Color.white;
            _playerScopeImage.DefaultColor = Color.white;
            _friendsScopeImage.DefaultColor = Color.white;
            _countryScopeImage.DefaultColor = Color.white;

            switch (_scope) {
                case GlobalPlayerScope.Global:
                    _globalScopeImage.DefaultColor = _selectedColor;
                    break;
                case GlobalPlayerScope.AroundPlayer:
                    _playerScopeImage.DefaultColor = _selectedColor;
                    break;
                case GlobalPlayerScope.Friends:
                    _friendsScopeImage.DefaultColor = _selectedColor;
                    break;
                case GlobalPlayerScope.Country:
                    _countryScopeImage.DefaultColor = _selectedColor;
                    break;
                case GlobalPlayerScope.Region:
                    _countryScopeImage.DefaultColor = _selectedColor;
                    break;
            }

            _page = 1;
            CheckPages();
            RefreshDelayed().RunTask();
        }

        private async Task RefreshDelayed() {

            ShowGlobalLoading(true);

            string localRequestId = Guid.NewGuid().ToString();
            _requestId = localRequestId;
            await Task.Delay(400);
            if(_scope == GlobalPlayerScope.Region) {
                _scope = GlobalPlayerScope.Country; // there is no around-region endpoint yet :( // remove when there is one
            }
            PlayerInfo[] globalLeaderboardData = await _globalLeaderboardService.GetPlayerList(_scope, _page);
            if (localRequestId == _requestId && globalLeaderboardData != null) {
                UpdateCells(globalLeaderboardData);
            }
        }

        private void UpdateCells(PlayerInfo[] players) {
            {

                _globalCells.Clear();
                _rankCells.Clear();
                _leaderboard.Data.Clear();

                int counter = 1;
                foreach (PlayerInfo player in players) {

                    string rank;
                    int localRank = counter + (((_page - 1) * 5) / 5) * 5;
                    if (_scope == GlobalPlayerScope.Country || _scope == GlobalPlayerScope.Friends) {
                        rank = string.Format("#{0:n0} (#{1:n0})", localRank, player.rank);
                    } else {
                        rank = string.Format("#{0:n0}", player.rank);
                    }

                    void onGlobalCellClicked(string identifier, string name) {
                        ShowProfile(identifier, name).RunTask();
                    }

                    _globalCells.Add(new GlobalCell(player.id, player.profilePicture, player.name, player.country, rank, GetWeekDifference(player.histories, player.rank), player.pp, onGlobalCellClicked));
                    counter++;
                }

                ShowGlobalLoading(false);

                foreach (GlobalCell globalCell in _globalCells) {
                    _rankCells.Add(globalCell);
                }
                _leaderboard.TableView.ReloadData();
            }
        }

        private int GetWeekDifference(string history, int currentRank) {
            if (currentRank == 0) {
                return 0;
            }
            string[] historyArray = history.Split(',');
            int lastWeek;
            if (historyArray.Length > 6) {
                lastWeek = Convert.ToInt32(historyArray[historyArray.Length - 1]);
            } else {
                lastWeek = Convert.ToInt32(historyArray[0]);
            }
            if (lastWeek == 999999) {
                return 0;
            }
            return lastWeek - currentRank;
        }

        public async Task ShowProfile(string playerId, string name) {

            _parserParams.EmitEvent("close-modals");
            _parserParams.EmitEvent("show-profile");
            _profileDetailView.SetLoadingState(true);
            _profileDetailView.name = name;
            try {
                await _profileDetailView.ShowProfile(playerId);
            } catch (Exception) {
                Plugin.Log.Error("Failed to load player stats, bad internet connection");
            }
        }

        private void CheckPages() {

            if (_page <= 1) {
                _upButton.interactable = false;
            } else {
                _upButton.interactable = true;
            }
        }

        private void ShowGlobalLoading(bool loading) {

            globalSet = !loading;
        }

        private void PageButtonClicked(bool down) {

            if (down) {
                _page++;
            } else {
                _page--;
            }
            CheckPages();
            RefreshDelayed().RunTask();
        }
    }
}

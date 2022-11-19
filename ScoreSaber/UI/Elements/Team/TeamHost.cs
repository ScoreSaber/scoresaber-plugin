#region

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace ScoreSaber.UI.Elements.Team {
    internal class TeamHost {
        private static readonly string _content;

        private bool _parsed;

        static TeamHost() {
            _content = Utilities.GetResourceContent(Assembly.GetExecutingAssembly(),
                "ScoreSaber.UI.Elements.Team.TeamHost.bsml");
        }

        public TeamHost(string teamName, IEnumerable<TeamUserInfo> profiles) {
            _teamName = teamName;
            this.profiles = profiles.Cast<object>().ToList();
        }

        public void Init() {
            if (_tabRoot != null) {
                Parse(_tabRoot.gameObject);
            } else {
                Plugin.Log.Info("tabRoot is null");
            }
        }

        public void Parse(GameObject parentGrid) {
            switch (_parsed) {
                case false: {
                    BSMLParser.instance.Parse(_content, parentGrid, this);
                    if (_grid != null) {
                        _grid.constraintCount = 3;
                        _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    }

                    _parsed = true;
                    break;
                }
            }
        }

        #region BSML Components

        [UIComponent("tab-root")] protected readonly RectTransform _tabRoot = null;

        [UIComponent("grid")] protected readonly GridLayoutGroup _grid = null;

        #endregion

        #region BSML Values

        [UIValue("profiles")] public List<object> profiles = new List<object>();

        [UIValue("needs-scroll-view")] protected bool needsScrollView => profiles.Count > 9;

        [UIValue("team-name")] public string _teamName { get; }

        #endregion
    }
}
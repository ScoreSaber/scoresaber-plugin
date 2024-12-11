using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using Newtonsoft.Json;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.UI.Elements.Team;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoreSaber.UI.Main.ViewControllers
{
    [HotReload]
    internal class TeamViewController : BSMLAutomaticViewController
    {

        [UIComponent("tab-selector")]
        protected readonly TabSelector _tabSelector = null;

        [UIValue("team-hosts")]
        protected readonly List<object> _teamHosts = new List<object>();

        [UIAction("#post-parse")]
        protected void Parsed() {

            _tabSelector.transform.localScale *= 0.75f;
        }

        protected override async void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {

            if (firstActivation) {

                _teamHosts.Clear();
                var team = await GetTeam();

                foreach (KeyValuePair<string, List<TeamMember>> member in team.TeamMembers) {
                    string teamName = member.Key.ToString();
                    if (teamName == "RT") {
                        teamName = " RT "; // this is to make the tab larger
                    }
                    TeamHost host = TeamToProfileHost(member.Value, teamName);
                    _teamHosts.Add(host);
                }
            }

            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            _tabSelector.TextSegmentedControl.didSelectCellEvent += DidSelect;
            if (_teamHosts.Count > 0) {
                TeamHost host = (TeamHost)_teamHosts[0];
                host.Init();
                foreach (TeamUserInfo profile in host.profiles) {
                    profile.LoadImage();
                }
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            _tabSelector.TextSegmentedControl.didSelectCellEvent -= DidSelect;
        }

        private void DidSelect(SegmentedControl segmentedControl, int pos) {

            var teamHost = _teamHosts[pos] as TeamHost;
            teamHost.Init();
            foreach (TeamUserInfo profile in teamHost.profiles) {
                profile.LoadImage();
            }
        }

        private TeamHost TeamToProfileHost(List<TeamMember> team, string teamName) {

            List<TeamUserInfo> host = new List<TeamUserInfo>();
            foreach (TeamMember member in team) {
                host.Add(new TeamUserInfo(member.ProfilePicture, member.Name, member.Discord, member.GitHub, member.Twitch, member.Twitter, member.YouTube));
            }
            return new TeamHost(teamName, host);
        }

        public async Task<ScoreSaberTeam> GetTeam() {

            var response = await Plugin.HttpInstance.GetRawAsync("https://raw.githubusercontent.com/Umbranoxio/ScoreSaber-Team/main/team.json");
            var teamData = JsonConvert.DeserializeObject<ScoreSaberTeam>(response);
            return teamData;
        }
    }
}
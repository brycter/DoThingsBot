using System;
using System.Collections.Generic;
using System.Globalization;

using DoThingsBot.Views.Pages;

using VirindiViewService.Controls;

namespace DoThingsBot.Views {
    public class ProfileManagerView : IDisposable {
        public readonly VirindiViewService.ViewProperties properties;
        public readonly VirindiViewService.ControlGroup controls;
        public readonly VirindiViewService.HudView view;

        private readonly HudList UIBuffProfileManagerAvailableSpells;
        private readonly HudList UIBuffProfileManagerProfileSpells;
        private readonly HudCombo UIBuffProfileManagerEdit;
        private readonly HudButton UIBuffProfileManagerMoveSpell;

        private int selectedProfileRow = -1;
        private int selectedAvailableRow = 0;
        private List<Spells.SpellClass> profileFamilyIds = new List<Spells.SpellClass>();

        public ProfileManagerView() {
            try {
                // Create the view
                VirindiViewService.XMLParsers.Decal3XMLParser parser = new VirindiViewService.XMLParsers.Decal3XMLParser();
                parser.ParseFromResource("DoThingsBot.Views.profileManagerView.xml", out properties, out controls);
                
                view = new VirindiViewService.HudView(properties, controls);
                view.ShowInBar = false;

                UIBuffProfileManagerEdit = (HudCombo)view["BuffProfileManagerEdit"];
                UIBuffProfileManagerAvailableSpells = (HudList)view["BuffProfileManagerAvailableSpells"];
                UIBuffProfileManagerProfileSpells = (HudList)view["BuffProfileManagerProfileSpells"];
                UIBuffProfileManagerMoveSpell = (HudButton)view["BuffProfileManagerMoveSpell"];

                view.VisibleChanged += View_VisibleChanged;
                view.Visible = true;

                UIBuffProfileManagerEdit.Change += UIBuffProfileManagerEdit_Change;
                UIBuffProfileManagerProfileSpells.Click += UIBuffProfileManagerProfileSpells_Click;
                UIBuffProfileManagerAvailableSpells.Click += UIBuffProfileManagerAvailableSpells_Click;

                UIBuffProfileManagerMoveSpell.Hit += UIBuffProfileManagerMoveSpell_Hit;

                FixListDisplay();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void UIBuffProfileManagerMoveSpell_Hit(object sender, EventArgs e) {
            if (selectedProfileRow >= 0) {
                int familyId = 0;
                var row = (HudList.HudListRowAccessor)UIBuffProfileManagerProfileSpells[selectedProfileRow];

                Int32.TryParse(((HudStaticText)row[4]).Text, out familyId);

                if (familyId == 0) return;

                profileFamilyIds.Remove((Spells.SpellClass)familyId);
                UIBuffProfileManagerProfileSpells.RemoveRow(selectedProfileRow);

                selectedAvailableRow = 0;
                selectedProfileRow = -1;

                RedrawAvailableList();
                FixListDisplay();
            }
            else {
                int familyId = 0;
                var row = (HudList.HudListRowAccessor)UIBuffProfileManagerAvailableSpells[selectedAvailableRow];

                Int32.TryParse(((HudStaticText)row[2]).Text, out familyId);

                if (familyId == 0) return;

                profileFamilyIds.Add((Spells.SpellClass)familyId);

                var newRow = (HudList.HudListRowAccessor)UIBuffProfileManagerProfileSpells.AddRow();
                var spell = Spells.GetExampleSpellByClass((Spells.SpellClass)familyId);
                var friendlyName = FriendlyName((Spells.SpellClass)familyId);
                var index = 


                ((HudPictureBox)newRow[0]).Image = spell.IconId;
                ((HudStaticText)newRow[1]).Text = friendlyName;
                ((HudPictureBox)newRow[2]).Image = 100673788; // up arrow
                ((HudPictureBox)newRow[3]).Image = 100673789; // down arrow
                ((HudStaticText)newRow[4]).Text = ((int)familyId).ToString();

                UIBuffProfileManagerAvailableSpells.RemoveRow(selectedAvailableRow);

                selectedAvailableRow = -1;
                selectedProfileRow = UIBuffProfileManagerProfileSpells.RowCount - 1;

                //RedrawAvailableList();
                FixListDisplay();
            }
        }

        private void RedrawAvailableList() {
            var values = Enum.GetValues(typeof(Spells.SpellClass));
            var index = 0;

            UIBuffProfileManagerAvailableSpells.ClearRows();

            foreach (Spells.SpellClass family in values) {
                if (family != Spells.SpellClass.UNKNOWN && !profileFamilyIds.Contains(family)) {
                    var spell = Spells.GetExampleSpellByClass(family);
                    var friendlyName = FriendlyName(family);

                    if (spell == null) continue;

                    HudList.HudListRowAccessor newRow = UIBuffProfileManagerAvailableSpells.AddRow();
                    ((HudPictureBox)newRow[0]).Image = spell.IconId;
                    ((HudStaticText)newRow[1]).Text = index == selectedAvailableRow ? "> " + friendlyName : friendlyName;
                    ((HudStaticText)newRow[2]).Text = ((int)family).ToString();
                    index++;
                }
            }
        }

        private void UIBuffProfileManagerAvailableSpells_Click(object sender, int row, int col) {
            HudList.HudListRowAccessor currentSelectedRow;
            HudList.HudListRowAccessor newSelectedRow = (HudList.HudListRowAccessor)UIBuffProfileManagerAvailableSpells[row];

            if (selectedAvailableRow >= 0 && selectedAvailableRow < UIBuffProfileManagerAvailableSpells.RowCount) {
                currentSelectedRow = (HudList.HudListRowAccessor)UIBuffProfileManagerAvailableSpells[selectedAvailableRow];

                if (currentSelectedRow != null) {
                    ((HudStaticText)currentSelectedRow[1]).Text = ((HudStaticText)currentSelectedRow[1]).Text.Replace("> ", "");
                }
            }

            if (!((HudStaticText)newSelectedRow[1]).Text.StartsWith("> ")) {
                ((HudStaticText)newSelectedRow[1]).Text = "> " + ((HudStaticText)newSelectedRow[1]).Text;
            }

            selectedAvailableRow = row;
            selectedProfileRow = -1;

            UIBuffProfileManagerMoveSpell.Text = ">>>";

            FixListDisplay();
        }

        private void UIBuffProfileManagerProfileSpells_Click(object sender, int row, int col) {
            HudList.HudListRowAccessor currentSelectedRow;
            HudList.HudListRowAccessor newSelectedRow = (HudList.HudListRowAccessor)UIBuffProfileManagerProfileSpells[row];

            if (selectedProfileRow >= 0 && selectedProfileRow < UIBuffProfileManagerProfileSpells.RowCount) {
                currentSelectedRow = (HudList.HudListRowAccessor)UIBuffProfileManagerProfileSpells[selectedProfileRow];

                if (currentSelectedRow != null) {
                    ((HudStaticText)currentSelectedRow[1]).Text = ((HudStaticText)currentSelectedRow[1]).Text.Replace("> ", "");
                }
            }

            if (!((HudStaticText)newSelectedRow[1]).Text.StartsWith("> ")) {
                ((HudStaticText)newSelectedRow[1]).Text = "> " + ((HudStaticText)newSelectedRow[1]).Text;
            }
            selectedProfileRow = row;

            var rowCount = UIBuffProfileManagerProfileSpells.RowCount;

            // up arrow
            if (col == 2 && row > 0) {
                HudList.HudListRowAccessor newRow = UIBuffProfileManagerProfileSpells.InsertRow(row - 1);
                HudList.HudListRowAccessor oldRow = (HudList.HudListRowAccessor)UIBuffProfileManagerProfileSpells[row + 1];
                ((HudPictureBox)newRow[0]).Image = ((HudPictureBox)oldRow[0]).Image;
                ((HudStaticText)newRow[1]).Text = ((HudStaticText)oldRow[1]).Text;
                ((HudPictureBox)newRow[2]).Image = row - 1 == 0 ? 100677592 : 100673788; // up arrow
                ((HudPictureBox)newRow[3]).Image = 100673789; // down arrow
                UIBuffProfileManagerProfileSpells.RemoveRow(row + 1);

                selectedProfileRow = row - 1;
            }

            // down arrow
            if (col == 3 && row != rowCount-1) {
                HudList.HudListRowAccessor newRow = UIBuffProfileManagerProfileSpells.InsertRow(row + 2);
                HudList.HudListRowAccessor oldRow = (HudList.HudListRowAccessor)UIBuffProfileManagerProfileSpells[row];
                ((HudPictureBox)newRow[0]).Image = ((HudPictureBox)oldRow[0]).Image;
                ((HudStaticText)newRow[1]).Text = ((HudStaticText)oldRow[1]).Text;
                ((HudPictureBox)newRow[2]).Image = row - 1 == 0 ? 100677592 : 100673788; // up arrow
                ((HudPictureBox)newRow[3]).Image = 100673789; // down arrow
                UIBuffProfileManagerProfileSpells.RemoveRow(row);

                selectedProfileRow = row + 1;
            }

            selectedAvailableRow = -1;

            UIBuffProfileManagerMoveSpell.Text = "<<<";

            FixListDisplay();
        }

        private string FriendlyName(Spells.SpellClass spellClass) {
            var parts = spellClass.ToString().Split('_');
            List<string> endParts = new List<string>();

            foreach (var part in parts) {
                endParts.Add(part[0] + part.Substring(1).ToLower());
            }

            return string.Join(" ", endParts.ToArray());
        }

        private void FixListDisplay() {
            var rowCount = UIBuffProfileManagerProfileSpells.RowCount;

            if (selectedProfileRow >= 0 && selectedAvailableRow >= 0) {
                selectedProfileRow = -1;
            }

            if (selectedAvailableRow >= 0) {
                UIBuffProfileManagerMoveSpell.Text = ">>>";
            }
            else {
                UIBuffProfileManagerMoveSpell.Text = "<<<";
            }

            for (var i = 0; i < rowCount; i++) {
                HudList.HudListRowAccessor row = (HudList.HudListRowAccessor)UIBuffProfileManagerProfileSpells[i];
                var text = ((HudStaticText)row[1]).Text.Replace("> ", "");
                ((HudStaticText)row[1]).Text = selectedProfileRow == i ? "> " + text : text;
                ((HudPictureBox)row[2]).Image = i == 0 ? 100677592 : 100673788; // up arrow
                ((HudPictureBox)row[3]).Image = i == rowCount - 1 ? 100677592 : 100673789; // down arrow
            }

            rowCount = UIBuffProfileManagerAvailableSpells.RowCount;

            for (var i = 0; i < rowCount; i++) {
                HudList.HudListRowAccessor row = (HudList.HudListRowAccessor)UIBuffProfileManagerAvailableSpells[i];
                var text = ((HudStaticText)row[1]).Text.Replace("> ", "");
                ((HudStaticText)row[1]).Text = selectedAvailableRow == i ? "> " + text : text;
            }
        }

        private void UIBuffProfileManagerEdit_Change(object sender, EventArgs e) {
            HudStaticText c = (HudStaticText)(UIBuffProfileManagerEdit[UIBuffProfileManagerEdit.Current]);
            LoadProfile(c.Text);
        }

        private void LoadProfile(string name) {
            try {
                if (string.IsNullOrEmpty(name)) {
                    ReloadProfiles();
                    return;
                }

                UIBuffProfileManagerProfileSpells.ClearRows();
                UIBuffProfileManagerAvailableSpells.ClearRows();

                if (selectedAvailableRow != -1) selectedAvailableRow = 0;
                if (selectedProfileRow != -1) selectedProfileRow = 0;

                var profile = Buffs.Buffs.GetProfile(name);

                var index = 0;

                profileFamilyIds.Clear();
                profileFamilyIds.AddRange(profile.directFamilyIds);

                foreach (var family in profileFamilyIds) {
                    var spell = Spells.GetExampleSpellByClass(family);
                    var friendlyName = FriendlyName(family);
                    HudList.HudListRowAccessor newRow = UIBuffProfileManagerProfileSpells.AddRow();
                    ((HudPictureBox)newRow[0]).Image = spell.IconId;
                    ((HudStaticText)newRow[1]).Text = index == selectedProfileRow ? "> " + friendlyName : friendlyName;
                    ((HudPictureBox)newRow[2]).Image = index == 0 ? 100677592 : 100673788; // up arrow
                    ((HudPictureBox)newRow[3]).Image = index == profile.directFamilyIds.Count-1 ? 100677592 : 100673789; // down arrow
                    ((HudStaticText)newRow[4]).Text = ((int)family).ToString();

                    index++;
                }

                RedrawAvailableList();

                FixListDisplay();
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private void View_VisibleChanged(object sender, EventArgs e) {
            if (view.Visible) {
                ReloadProfiles();
            }
        }

        public void ReloadProfiles() {
            UIBuffProfileManagerProfileSpells.ClearRows();
            UIBuffProfileManagerAvailableSpells.ClearRows();
            UIBuffProfileManagerEdit.Clear();

            if (selectedAvailableRow != -1) selectedAvailableRow = 0;
            if (selectedProfileRow != -1) selectedProfileRow = 0;

            LoadProfileList();
            FixListDisplay();
        }

        private void LoadProfileList() {
            UIBuffProfileManagerEdit.AddItem("", "");

            foreach (var profileName in Buffs.Buffs.profiles.Keys) {
                if (Buffs.Buffs.profiles[profileName].IsAutoGenerated()) continue;
                UIBuffProfileManagerEdit.AddItem(profileName, profileName);
            }
        }

        private bool disposed;

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    view.VisibleChanged -= View_VisibleChanged;
                    UIBuffProfileManagerEdit.Change -= UIBuffProfileManagerEdit_Change;
                    UIBuffProfileManagerProfileSpells.Click -= UIBuffProfileManagerProfileSpells_Click;
                    //Remove the view
                    if (view != null) view.Dispose();
                }
                
                disposed = true;
            }
        }
    }
}

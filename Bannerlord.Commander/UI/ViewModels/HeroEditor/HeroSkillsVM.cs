using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Bannerlord.Commander.UI.ViewModels.HeroEditor
{
    /// <summary>
    /// ViewModel for hero skills display.
    /// Shows all 18 skills grouped by their 6 attributes (Vigor, Control, Endurance, Cunning, Social, Intelligence).
    /// </summary>
    public class HeroSkillsVM : ViewModel
    {
        #region Private Fields

        private Hero _hero;
        private int _level;
        private MBBindingList<HeroAttributeGroupVM> _attributeGroups;

        #endregion

        #region Constructor

        public HeroSkillsVM()
        {
            AttributeGroups = new MBBindingList<HeroAttributeGroupVM>();
            Clear();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refreshes the ViewModel with data from the specified hero.
        /// </summary>
        /// <param name="hero">The hero to display skills for</param>
        public void RefreshForHero(Hero hero)
        {
            _hero = hero;
            
            if (_hero != null)
            {
                Level = _hero.Level;
                InitializeSkillsAndAttributes();
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Clears all data from the ViewModel.
        /// </summary>
        public void Clear()
        {
            _hero = null;
            Level = 0;
            AttributeGroups.Clear();
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            
            if (AttributeGroups != null)
            {
                foreach (var group in AttributeGroups)
                {
                    group?.OnFinalize();
                }
            }
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the hero's level.
        /// </summary>
        [DataSourceProperty]
        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value, nameof(Level));
        }

        /// <summary>
        /// Gets the list of attribute groups, each containing 3 skills.
        /// </summary>
        [DataSourceProperty]
        public MBBindingList<HeroAttributeGroupVM> AttributeGroups
        {
            get => _attributeGroups;
            private set => SetProperty(ref _attributeGroups, value, nameof(AttributeGroups));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes all 6 attributes with their 3 skills each (18 total).
        /// Skills are displayed regardless of their value.
        /// </summary>
        private void InitializeSkillsAndAttributes()
        {
            AttributeGroups.Clear();
            
            if (_hero == null)
                return;
            
            // Define all 6 attribute groups with their 3 skills each
            var attributeDefinitions = new[]
            {
                new { AttributeName = "Vigor", AttributeId = "VIG", SkillIds = new[] { "OneHanded", "TwoHanded", "Polearm" } },
                new { AttributeName = "Control", AttributeId = "CTR", SkillIds = new[] { "Bow", "Crossbow", "Throwing" } },
                new { AttributeName = "Endurance", AttributeId = "END", SkillIds = new[] { "Riding", "Athletics", "Crafting" } },
                new { AttributeName = "Cunning", AttributeId = "CNG", SkillIds = new[] { "Scouting", "Tactics", "Roguery" } },
                new { AttributeName = "Social", AttributeId = "SOC", SkillIds = new[] { "Charm", "Leadership", "Trade" } },
                new { AttributeName = "Intelligence", AttributeId = "INT", SkillIds = new[] { "Steward", "Medicine", "Engineering" } }
            };
            
            foreach (var attrDef in attributeDefinitions)
            {
                var attributeGroup = new HeroAttributeGroupVM(
                    attrDef.AttributeName,
                    attrDef.AttributeId,
                    _hero,
                    attrDef.SkillIds);
                
                AttributeGroups.Add(attributeGroup);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to reduce boilerplate in int property setters.
        /// </summary>
        private bool SetProperty(ref int field, int value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Helper method for MBBindingList property setters.
        /// </summary>
        private bool SetProperty(ref MBBindingList<HeroAttributeGroupVM> field, MBBindingList<HeroAttributeGroupVM> value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChangedWithValue(value, propertyName);
            return true;
        }

        #endregion
    }

    /// <summary>
    /// ViewModel representing an attribute group with its associated skills.
    /// </summary>
    public class HeroAttributeGroupVM : ViewModel
    {
        #region Private Fields

        private string _attributeName;
        private string _attributeId;
        private int _attributeValue;
        private MBBindingList<HeroSkillItemVM> _skills;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new attribute group with skills.
        /// </summary>
        /// <param name="attributeName">Display name of the attribute (e.g., "Vigor")</param>
        /// <param name="attributeId">Short identifier for the attribute (e.g., "VIG")</param>
        /// <param name="hero">The hero to get skill values from</param>
        /// <param name="skillIds">Array of skill IDs belonging to this attribute</param>
        public HeroAttributeGroupVM(string attributeName, string attributeId, Hero hero, string[] skillIds)
        {
            AttributeName = attributeName;
            AttributeId = attributeId;
            Skills = new MBBindingList<HeroSkillItemVM>();
            
            if (hero != null && skillIds != null)
            {
                // Calculate attribute value (based on related skills)
                int totalSkillValue = 0;
                int skillCount = 0;
                
                foreach (var skillId in skillIds)
                {
                    // Use SkillObject to find skill by StringId
                    var skill = Game.Current.ObjectManager.GetObjectTypeList<SkillObject>()
                        .FirstOrDefault(s => s.StringId == skillId);
                    if (skill != null)
                    {
                        int skillValue = hero.GetSkillValue(skill);
                        totalSkillValue += skillValue;
                        skillCount++;
                        
                        var skillVM = new HeroSkillItemVM(skill, skillValue);
                        Skills.Add(skillVM);
                    }
                }
                
                // Attribute value is the average of its skills divided by 10 (Bannerlord formula)
                AttributeValue = skillCount > 0 ? totalSkillValue / skillCount / 10 : 0;
            }
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the attribute display name.
        /// </summary>
        [DataSourceProperty]
        public string AttributeName
        {
            get => _attributeName;
            private set => SetProperty(ref _attributeName, value, nameof(AttributeName));
        }

        /// <summary>
        /// Gets the attribute short identifier.
        /// </summary>
        [DataSourceProperty]
        public string AttributeId
        {
            get => _attributeId;
            private set => SetProperty(ref _attributeId, value, nameof(AttributeId));
        }

        /// <summary>
        /// Gets the calculated attribute value.
        /// </summary>
        [DataSourceProperty]
        public int AttributeValue
        {
            get => _attributeValue;
            private set => SetProperty(ref _attributeValue, value, nameof(AttributeValue));
        }

        /// <summary>
        /// Gets the list of skills in this attribute group.
        /// </summary>
        [DataSourceProperty]
        public MBBindingList<HeroSkillItemVM> Skills
        {
            get => _skills;
            private set => SetProperty(ref _skills, value, nameof(Skills));
        }

        #endregion

        #region Helper Methods

        private bool SetProperty(ref string field, string value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChangedWithValue(value, propertyName);
            return true;
        }

        private bool SetProperty(ref int field, int value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private bool SetProperty(ref MBBindingList<HeroSkillItemVM> field, MBBindingList<HeroSkillItemVM> value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChangedWithValue(value, propertyName);
            return true;
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            
            if (Skills != null)
            {
                foreach (var skill in Skills)
                {
                    skill?.OnFinalize();
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// ViewModel representing a single skill with its value.
    /// </summary>
    public class HeroSkillItemVM : ViewModel
    {
        #region Private Fields

        private string _skillId;
        private string _skillName;
        private int _skillValue;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new skill item.
        /// </summary>
        /// <param name="skill">The skill object</param>
        /// <param name="skillValue">The hero's value in this skill</param>
        public HeroSkillItemVM(SkillObject skill, int skillValue)
        {
            if (skill != null)
            {
                SkillId = skill.StringId;
                SkillName = skill.Name?.ToString() ?? skill.StringId;
                SkillValue = skillValue;
            }
        }

        #endregion

        #region DataSource Properties

        /// <summary>
        /// Gets the skill string identifier (used for icon display).
        /// </summary>
        [DataSourceProperty]
        public string SkillId
        {
            get => _skillId;
            private set => SetProperty(ref _skillId, value, nameof(SkillId));
        }

        /// <summary>
        /// Gets the skill display name.
        /// </summary>
        [DataSourceProperty]
        public string SkillName
        {
            get => _skillName;
            private set => SetProperty(ref _skillName, value, nameof(SkillName));
        }

        /// <summary>
        /// Gets the skill value (0-300 typically).
        /// </summary>
        [DataSourceProperty]
        public int SkillValue
        {
            get => _skillValue;
            private set => SetProperty(ref _skillValue, value, nameof(SkillValue));
        }

        #endregion

        #region Helper Methods

        private bool SetProperty(ref string field, string value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChangedWithValue(value, propertyName);
            return true;
        }

        private bool SetProperty(ref int field, int value, string propertyName)
        {
            if (field == value)
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}

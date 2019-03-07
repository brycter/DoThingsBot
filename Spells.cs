using System;
using System.Collections.Generic;
using System.Text;

using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Filters;

namespace DoThingsBot {
    public static class Spells {
        public static string[] MANA_RECOVERY_SPELLS = new string[]
        {
            "Stamina to Mana Self I",
            "Stamina to Mana Self II",
            "Stamina to Mana Self III",
            "Stamina to Mana Self IV",
            "Stamina to Mana Self V",
            "Stamina to Mana Self VI",
            "Meditative Trance",
            "Incantation of Stamina to Mana Self",
        };

        public static string[] STAMINA_RECOVERY_SPELLS = new string[]
        {
            "Revitalize Self I",
            "Revitalize Self II",
            "Revitalize Self III",
            "Revitalize Self IV",
            "Revitalize Self V",
            "Revitalize Self VI",
            "Robustification",
            "Incantation of Revitalize Self",
        };

        public enum SpellClass {
            UNKNOWN = 0,
            STRENGTH = 1,
            WEAKNESS = 2,
            ENDURANCE = 3,
            FRAILTY = 4,
            QUICKNESS = 5,
            SLOWNESS = 6,
            COORDINATION = 7,
            CLUMSINESS = 8,
            FOCUS = 9,
            BAFFLEMENT = 10,
            WILLPOWER = 11,
            FEEBLEMIND = 12,
            CONCENTRATION = 13,
            BRILLIANCE = 15,
            LIGHT_WEAPON_MASTERY = 17,
            LIGHT_WEAPON_INEPTITUDE = 18,
            MISSILE_WEAPON_MASTERY = 19,
            MISSILE_WEAPON_INEPTITUDE = 20,
            FINESSE_WEAPON_MASTERY = 23,
            FINESSE_WEAPON_INEPTITUDE = 24,
            HEAVY_WEAPON_MASTERY = 31,
            HEAVY_WEAPON_INEPTITUDE = 32,
            INVULNERABILITY = 37,
            VULNERABILITY = 38,
            IMPREGNABILITY = 39,
            DEFENSELESSNESS = 40,
            MAGIC_RESISTANCE = 41,
            MAGIC_YIELD = 42,
            CREATURE_ENCHANTMENT_MASTERY = 43,
            CREATURE_ENCHANTMENT_INEPTITUDE = 44,
            ITEM_ENCHANTMENT_MASTERY = 45,
            ITEM_ENCHANTMENT_INEPTITUDE = 46,
            LIFE_MAGIC_MASTERY = 47,
            LIFE_MAGIC_INEPTITUDE = 48,
            WAR_MAGIC_MASTERY = 49,
            WAR_MAGIC_INEPTITUDE = 50,
            MANA_CONVERSION_MASTERY = 51,
            MANA_CONVERSION_INEPTITUDE = 52,
            ARCANE_ENLIGHTENMENT = 53,
            ARCANE_BENIGHTEDNESS = 54,
            ARMOR_TINKERING_EXPERTISE = 55,
            ARMOR_TINKERING_IGNORANCE = 56,
            ITEM_TINKERING_EXPERTISE = 57,
            ITEM_TINKERING_IGNORANCE = 58,
            MAGIC_ITEM_TINKERING_EXPERTISE = 59,
            MAGIC_ITEM_TINKERING_IGNORANCE = 60,
            WEAPON_TINKERING_EXPERTISE = 61,
            WEAPON_TINKERING_IGNORANCE = 62,
            MONSTER_ATTUNEMENT = 63,
            MONSTER_UNFAMILIARITY = 64,
            DECEPTION_MASTERY = 65,
            DECEPTION_INEPTITUDE = 66,
            HEAL_OTHER = 67,
            HEALING_INEPTITUDE = 68,
            JUMPING_MASTERY = 69,
            JUMPING_INEPTITUDE = 70,
            LEADERSHIP_MASTERY = 71,
            LEADERSHIP_INEPTITUDE = 72,
            LOCKPICK_MASTERY = 73,
            LOCKPICK_INEPTITUDE = 74,
            FEALTY = 75,
            FAITHLESSNESS = 76,
            SPRINT = 77,
            LEADEN_FEET = 78,
            HEAL_SELF = 79,
            HARM = 80,
            REVITALIZE = 81,
            ENFEEBLE = 82,
            MANA_BOOST = 83,
            MANA_DRAIN = 84,
            DRAIN_HEALTH = 87,
            INFUSE_HEALTH = 88,
            DRAIN_STAMINA = 89,
            INFUSE_STAMINA = 90,
            DRAIN_MANA = 91,
            INFUSE_MANA = 92,
            REGENERATION = 93,
            FESTER = 94,
            REJUVENATION = 95,
            EXHAUSTION = 96,
            MANA_RENEWAL = 97,
            MANA_DEPLETION = 98,
            ACID_PROTECTION = 101,
            ACID_VULNERABILITY = 102,
            BLUDGEONING_PROTECTION = 103,
            BLUDGEONING_VULNERABILITY = 104,
            COLD_PROTECTION = 105,
            COLD_VULNERABILITY = 106,
            LIGHTNING_PROTECTION = 107,
            LIGHTNING_VULNERABILITY = 108,
            FIRE_PROTECTION = 109,
            FIRE_VULNERABILITY = 110,
            PIERCING_PROTECTION = 111,
            PIERCING_VULNERABILITY = 112,
            BLADE_PROTECTION = 113,
            BLADE_VULNERABILITY = 114,
            ARMOR = 115,
            IMPERIL = 116,
            ACID_STREAM = 117,
            SHOCK_WAVE = 118,
            FROST_BOLT = 119,
            LIGHTNING_BOLT = 120,
            FLAME_BOLT = 121,
            MAGIC_BOLT = 122,
            WHIRLING_BLADE = 123,
            ACID_BLAST = 131,
            SHOCK_BLAST = 132,
            FROST_BLAST = 133,
            LIGHTNING_BLAST = 134,
            FLAME_BLAST = 135,
            FORCE_BLAST = 136,
            BLADE_BLAST = 137,
            AURA_OF_HEART_SEEKER = 152,
            TURN_BLADE = 153,
            AURA_OF_BLOOD_DRINKER = 154,
            BLOOD_LOATHER = 155,
            AURA_OF_DEFENDER = 156,
            LURE_BLADE = 157,
            AURA_OF_SWIFT_KILLER = 158,
            LEADEN_WEAPON = 159,
            IMPENETRABILITY = 160,
            BRITTLEMAIL = 161,
            ACID_BANE = 162,
            ACID_LURE = 163,
            BLUDGEON_BANE = 164,
            BLUDGEON_LURE = 165,
            FROST_BANE = 166,
            FROST_LURE = 167,
            LIGHTNING_BANE = 168,
            LIGHTNING_LURE = 169,
            FLAME_BANE = 170,
            FLAME_LURE = 171,
            PIERCING_BANE = 172,
            PIERCING_LURE = 173,
            BLADE_BANE = 174,
            BLADE_LURE = 175,
            LESSER_BLUDGEONING_DURANCE = 176,
            LESSER_SLASHING_DURANCE = 178,
            LESSER_PIERCING_DURANCE = 180,
            GREATER_STIMULATION_DURANCE = 182,
            GREATER_STASIS_DURANCE = 184,
            GREATER_CONSUMPTION_DURANCE = 186,
            GREATER_DECAY_DURANCE = 188,
            HIEROMANCERS_WARD = 190,
            STRENGTHEN_LOCK = 192,
            WEAKEN_LOCK = 193,
            HERMETIC_VOID = 194,
            AURA_OF_HERMETIC_LINK = 195,
            KIVIK_LIRS_BOON = 196,
            DANCE = 198,
            PRIMARY_PORTAL_TIE = 200,
            PRIMARY_PORTAL_RECALL = 201,
            SUMMON_PRIMARY_PORTAL = 203,
            VITAE = 204,
            PERSON_ATTUNEMENT = 205,
            PERSON_UNFAMILIARITY = 206,
            ACID_VOLLEY = 207,
            BLUDGEONING_VOLLEY = 208,
            FROST_VOLLEY = 209,
            LIGHTNING_VOLLEY = 210,
            FLAME_VOLLEY = 211,
            FORCE_VOLLEY = 212,
            BLADE_VOLLEY = 213,
            PORTAL_SENDING = 214,
            LIFESTONE_SENDING = 215,
            COOKING_MASTERY = 216,
            COOKING_INEPTITUDE = 217,
            FLETCHING_MASTERY = 218,
            FLETCHING_INEPTITUDE = 219,
            ALCHEMY_INEPTITUDE = 220,
            ALCHEMY_MASTERY = 221,
            SEARING_DISC = 222,
            TECTONIC_RIFTS = 223,
            HALO_OF_FROST = 224,
            EYE_OF_THE_STORM = 225,
            CASSIUS_RING_OF_FIRE = 226,
            NUHMUDIRAS_SPINES = 227,
            HORIZONS_BLADES = 228,
            BLISTERING_CREEPER = 229,
            HAMMERING_CRAWLER = 230,
            FOONKIS_GLACIAL_FLOE = 231,
            OS_WALL = 232,
            SLITHERING_FLAMES = 233,
            SPIKE_STRAFE = 234,
            BED_OF_BLADES = 235,
            TORRENTIAL_ACID = 236,
            STONE_FISTS = 237,
            AVALANCHE = 238,
            LIGHTNING_BARRAGE = 239,
            FIRESTORM = 240,
            SPLINTERFALL = 241,
            SQUALL_OF_SWORDS = 242,
            ACID_STREAK = 243,
            SHOCK_WAVE_STREAK = 244,
            FROST_STREAK = 245,
            LIGHTNING_STREAK = 246,
            FLAME_STREAK = 247,
            FORCE_STREAK = 248,
            WHIRLING_BLADE_STREAK = 249,
            EVAPORATE_ALL_MAGIC_OTHER = 250,
            BEAST_MURMUR = 251,
            GRIP_OF_INSTRUMENTALITY = 253,
            GLIMPSE_OF_ANNIHILATION = 255,
            MAJOR_HEALTH_GAIN = 257,
            MAJOR_MANA_GAIN = 259,
            PROWESS = 261,
            VIGOR = 263,
            HASTE = 265,
            CAUTION = 267,
            DETERMINATION = 269,
            SERENITY = 271,
            WARRIORS_LESSER_VITALITY = 273,
            WARRIORS_LESSER_VIGOR = 275,
            WIZARDS_LESSER_INTELLECT = 277,
            LIFE_GIVER = 279,
            EVIL_THIRST = 280,
            STAMINA_GIVER = 281,
            MANA_GIVER = 283,
            MALEDICTION = 284,
            ACID_WARD = 285,
            FLAME_WARD = 287,
            FROST_WARD = 289,
            LIGHTNING_WARD = 291,
            OBEDIENCE = 293,
            TIMARUS_SHELTER = 295,
            GREATER_GROWTH = 309,
            GREATER_CASCADE = 311,
            GREATER_STONE_CLIFFS = 321,
            GREATER_THORNS = 323,
            EYE_OF_THE_HUNTER = 325,
            STRONG_PULL = 327,
            WEAPON_FAMILIARITY = 329,
            PRECISE = 331,
            MAJOR_ALCHEMICAL_PROWESS = 333,
            MAJOR_ARCANE_PROWESS = 335,
            MAJOR_ARMOR_TINKERING_EXPERTISE = 337,
            MAJOR_COOKING_PROWESS = 339,
            MAJOR_DECEPTION_PROWESS = 343,
            MAJOR_FEALTY = 345,
            MAJOR_FLETCHING_PROWESS = 347,
            MAJOR_HEALING_PROWESS = 349,
            MAJOR_INVULNERABILITY = 351,
            MAJOR_ITEM_TINKERING_EXPERTISE = 353,
            MAJOR_JUMPING_PROWESS = 355,
            MAJOR_LIFE_MAGIC_APTITUDE = 357,
            MAJOR_LOCKPICK_PROWESS = 359,
            MAJOR_MAGIC_ITEM_TINKERING_EXPERTISE = 361,
            MAJOR_MANA_CONVERSION_PROWESS = 363,
            MAJOR_MONSTER_ATTUNEMENT = 365,
            MAJOR_PERSON_ATTUNEMENT = 367,
            MAJOR_SPRINT = 369,
            MAJOR_HEAVY_WEAPON_APTITUDE = 371,
            MAJOR_WEAPON_TINKERING_EXPERTISE = 377,
            MAJOR_ARMOR = 379,
            MAJOR_ACID_BANE = 381,
            MAJOR_BLUDGEONING_BANE = 383,
            MAJOR_FLAME_BANE = 385,
            MAJOR_FROST_BANE = 387,
            GREATER_ROCKSLIDE = 389,
            MAJOR_IMPENETRABILITY = 391,
            MAJOR_PIERCING_BANE = 393,
            MAJOR_SLASHING_BANE = 395,
            MAJOR_STORM_BANE = 397,
            MAJOR_SWIFT_HUNTER = 399,
            MAJOR_BLUDGEONING_WARD = 401,
            MAJOR_SLASHING_WARD = 403,
            MAJOR_PIERCING_WARD = 405,
            MAJOR_STAMINA_GAIN = 407,
            BOOM_BLACK_FIREWORK_OUT = 409,
            PLAGUE = 410,
            SCOURGE = 411,
            DEPLETION = 412,
            GRACE_OF_THE_UNICORN = 413,
            POWER_OF_THE_DRAGON = 414,
            SPLENDOR_OF_THE_FIREBIRD = 415,
            ENDURANCE_OF_THE_ABYSS = 416,
            WILL_OF_THE_QUIDDITY = 417,
            UNCANNY_DODGE = 418,
            FORESIGHT = 419,
            DISPERSION = 420,
            AERFALLES_TOUCH = 421,
            BENEDICTION_OF_IMMORTALITY = 422,
            CLOSING_OF_THE_GREAT_DIVIDE = 423,
            AERFALLES_ENFORCEMENT = 424,
            MINOR_HERMETIC_LINK = 425,
            FANATICISM = 426,
            WEAVE_OF_CHORIZITE = 428,
            SACROSANCT_TOUCH = 429,
            DIVINE_MANIPULATION = 430,
            CONSECRATION = 431,
            GIFT_OF_THE_FIAZHAT = 432,
            LESSER_VISION_BEYOND_THE_GRAVE = 433,
            EYES_BEYOND_THE_MIST = 434,
            ARCANUM_SALVAGING = 435,
            MINOR_SALVAGING_APTITUDE = 437,
            KERNS_BOON = 438,
            RANGERS_BOON = 439,
            FENCERS_BOON = 442,
            SOLDIERS_BOON = 447,
            PRODIGAL_ACID_PROTECTION = 448,
            PRODIGAL_ACID_BANE = 449,
            PRODIGAL_ALCHEMY_MASTERY = 450,
            PRODIGAL_HERMETIC_LINK = 451,
            PRODIGAL_ARMOR_EXPERTISE = 452,
            PRODIGAL_ITEM_EXPERTISE = 453,
            PRODIGAL_MAGIC_ITEM_EXPERTISE = 454,
            PRODIGAL_WEAPON_EXPERTISE = 455,
            PRODIGAL_ARCANE_ENLIGHTENMENT = 456,
            PRODIGAL_ARMOR = 457,
            PRODIGAL_IMPENETRABILITY = 458,
            PRODIGAL_MONSTER_ATTUNEMENT = 459,
            PRODIGAL_PERSON_ATTUNEMENT = 460,
            PRODIGAL_HEART_SEEKER = 461,
            PRODIGAL_LIGHT_WEAPON_MASTERY = 462,
            PRODIGAL_BLUDGEON_PROTECTION = 463,
            PRODIGAL_BLUDGEON_BANE = 464,
            PRODIGAL_MISSILE_WEAPON_MASTERY = 465,
            PRODIGAL_COLD_PROTECTION = 466,
            PRODIGAL_FROST_BANE = 467,
            PRODIGAL_COOKING_MASTERY = 468,
            PRODIGAL_COORDINATION = 469,
            PRODIGAL_CREATURE_ENCHANTMENT_MASTERY = 470,
            PRODIGAL_FINESSE_WEAPON_MASTERY = 472,
            PRODIGAL_BLOOD_DRINKER = 473,
            PRODIGAL_DECEPTION_MASTERY = 474,
            PRODIGAL_DEFENDER = 475,
            PRODIGAL_LIGHTNING_PROTECTION = 476,
            PRODIGAL_LIGHTNING_BANE = 477,
            PRODIGAL_ENDURANCE = 478,
            PRODIGAL_FIRE_PROTECTION = 479,
            PRODIGAL_FLAME_BANE = 480,
            PRODIGAL_FLETCHING_MASTERY = 481,
            PRODIGAL_FOCUS = 482,
            PRODIGAL_HEALING_MASTERY = 483,
            PRODIGAL_REGENERATION = 484,
            PRODIGAL_ITEM_ENCHANTMENT_MASTERY = 485,
            PRODIGAL_JUMPING_MASTERY = 486,
            PRODIGAL_LEADERSHIP_MASTERY = 487,
            PRODIGAL_LIFE_MAGIC_MASTERY = 488,
            PRODIGAL_LOCKPICK_MASTERY = 489,
            PRODIGAL_FEALTY = 490,
            PRODIGAL_MAGIC_RESISTANCE = 492,
            PRODIGAL_MANA_RENEWAL = 493,
            PRODIGAL_MANA_CONVERSION_MASTERY = 494,
            PRODIGAL_INVULNERABILITY = 495,
            PRODIGAL_IMPREGNABILITY = 496,
            PRODIGAL_PIERCING_PROTECTION = 497,
            PRODIGAL_PIERCING_BANE = 498,
            PRODIGAL_QUICKNESS = 499,
            PRODIGAL_SPRINT = 500,
            PRODIGAL_WILLPOWER = 501,
            PRODIGAL_BLADE_PROTECTION = 502,
            PRODIGAL_BLADE_BANE = 503,
            PRODIGAL_REJUVENATION = 506,
            PRODIGAL_STRENGTH = 507,
            PRODIGAL_HEAVY_WEAPON_MASTERY = 508,
            PRODIGAL_WAR_MAGIC_MASTERY = 511,
            PRODIGAL_SWIFT_KILLER = 512,
            INKY_ARMOR = 513,
            FIUN_RESISTANCE = 514,
            FIUN_FLEE = 515,
            FIUN_EFFICIENCY = 516,
            BURNING_SPIRIT = 517,
            SHADOW_TOUCH = 518,
            BLACKMOORS_FAVOR = 519,
            ASHERONS_BENEDICTION = 520,
            INVOCATION_OF_THE_BLACK_BOOK = 521,
            DARK_REFLEXES = 522,
            ARTISAN_ALCHEMISTS_INSPIRATION = 523,
            ARTISAN_COOKS_INSPIRATION = 524,
            ARTISAN_FLETCHERS_INSPIRATION = 525,
            ARTISAN_LOCKPICKERS_INSPIRATION = 526,
            MUCOR_MANA_WELL = 527,
            AURLANAAS_RESOLVE = 528,
            SUPREMACY = 529,
            BLEED_OTHER = 530,
            POISON = 536,
            NOVICE_SOLDIERS_HEAVY_WEAPON_APTITUDE = 538,
            NOVICE_SOLDIERS_LIGHT_WEAPON_APTITUDE = 539,
            NOVICE_SOLDIERS_FINESSE_WEAPON_APTITUDE = 540,
            NOVICE_ARCHERS_MISSILE_WEAPON_APTITUDE = 545,
            NOVICE_ARTIFEXS_ITEM_APTITUDE = 548,
            NOVICE_ENCHANTERS_CREATURE_APTITUDE = 549,
            NOVICE_WARLOCKS_WAR_MAGIC_APTITUDE = 550,
            NOVICE_THEURGES_LIFE_MAGIC_APTITUDE = 551,
            NOVICE_GUARDIANS_INVULNERABILITY = 552,
            NOVICE_WAYFARERS_IMPREGNABILITY = 553,
            NOVICE_NEGATORS_MAGIC_RESISTANCE = 554,
            NOVICE_CHALLENGERS_REJUVENATION = 555,
            NOVICE_CHEFS_COOKING_APTITUDE = 556,
            NOVICE_HUNTSMANS_FLETCHING_APTITUDE = 557,
            NOVICE_LOCKSMITHS_LOCKPICK_APTITUDE = 558,
            NOVICE_CONCOCTORS_ALCHEMY_APTITUDE = 559,
            NOVICE_SCAVENGERS_SALVAGING_APTITUDE = 560,
            NOVICE_ARMORERS_ARMOR_TINKERING_APTITUDE = 561,
            NOVICE_SWORDSMITHS_WEAPON_TINKERING_APTITUDE = 562,
            NOVICE_INVENTORS_ITEM_TINKERING_APTITUDE = 563,
            NOVICE_ARCANISTS_MAGIC_ITEM_TINKERING_APTITUDE = 564,
            YEOMANS_LOYALTY = 565,
            NOVICE_BRUTES_STRENGTH = 566,
            NOVICE_HEROS_ENDURANCE = 567,
            NOVICE_DUELISTS_COORDINATION = 568,
            NOVICE_ROVERS_QUICKNESS = 569,
            NOVICE_SAGES_FOCUS = 570,
            NOVICE_ADHERENTS_WILLPOWER = 571,
            APPRENTICE_SURVIVORS_HEALTH = 572,
            APPRENTICE_TRACKERS_STAMINA = 573,
            APPRENTICE_CLAIRVOYANTS_MANA = 574,
            NOVICE_MESSENGERS_SPRINT_APTITUDE = 575,
            NOVICE_LEAPERS_JUMPING_APTITUDE = 576,
            INCIDENTAL_SLASHING_RESISTANCE = 577,
            INCIDENTAL_BLUDGEONING_RESISTANCE = 578,
            INCIDENTAL_PIERCING_RESISTANCE = 579,
            INCIDENTAL_FLAME_RESISTANCE = 580,
            INCIDENTAL_ACID_RESISTANCE = 581,
            INCIDENTAL_FROST_RESISTANCE = 582,
            INCIDENTAL_LIGHTNING_RESISTANCE = 583,
            NIMBLE_FINGERS_LOCKPICK = 584,
            NIMBLE_FINGERS_FLETCHING = 585,
            NIMBLE_FINGERS_COOKING = 586,
            NIMBLE_FINGERS_ALCHEMY = 587,
            ASSASSINS_ALCHEMY_KIT = 592,
            INCANTATION_OF_TWO_HANDED_COMBAT_MASTERY_SELF = 593,
            TWO_HANDED_COMBAT_INEPTITUDE = 594,
            TWO_HANDED_FIGHTERS_BOON = 597,
            SPECTRAL_TWO_HANDED_COMBAT_MASTERY = 598,
            NOVICE_SOLDIERS_TWO_HANDED_COMBAT_APTITUDE = 599,
            MODERATE_ITEM_TINKERING_EXPERTISE = 602,
            ANSWER_OF_LOYALTY_MANA = 607,
            ANSWER_OF_LOYALTY_STAMINA = 608,
            CALL_OF_LEADERSHIP = 609,
            AUGMENTED_DAMAGE = 610,
            AUGMENTED_DAMAGE_REDUCTION = 611,
            AUGMENTED_HEALTH = 612,
            AUGMENTED_STAMINA = 613,
            AUGMENTED_MANA = 614,
            AUGMENTED_UNDERSTANDING = 615,
            VIRINDI_WHISPER = 616,
            SPECTRAL_FOUNTAIN_SIP = 617,
            SIGIL_OF_DESTRUCTION = 620,
            SIGIL_OF_DEFENSE = 621,
            SIGIL_OF_VIGOR_I_HEALTH = 623,
            SIGIL_OF_VIGOR_I_STAMINA = 624,
            SIGIL_OF_VIGOR_I_MANA = 625,
            SIGIL_OF_FURY_I_CRITICAL_DAMAGE = 626,
            SIGIL_OF_GROWTH = 627,
            SURGE_OF_DESTRUCTION = 628,
            SURGE_OF_PROTECTION = 629,
            SURGE_OF_REGENERATION = 630,
            SURGE_OF_AFFLICTION = 631,
            RARE_DAMAGE_BOOST = 633,
            RARE_DAMAGE_REDUCTION = 634,
            SIGIL_OF_FURY_I_ENDURANCE = 635,
            BAELZHARONS_CURSE_OF_DESTRUCTION = 636,
            BAELZHARONS_CURSE_OF_MINOR_DESTRUCTION = 637,
            CORRUPTION = 638,
            BAELZHARONS_NETHER_STREAK = 639,
            BAELZHARONS_NETHER_ARC = 640,
            CLOUDED_SOUL = 641,
            WEAKENING_CURSE = 642,
            FESTERING_CURSE = 643,
            VOID_MAGIC_INEPTITUDE = 644,
            VOID_MAGIC_MASTERY = 645,
            MINOR_VOID_MAGIC_APTITUDE = 646,
            NOVICE_SHADOWS_VOID_MAGIC_APTITUDE = 647,
            SPECTRAL_VOID_MAGIC_MASTERY = 648,
            CORRUPTORS_BOON = 649,
            SURGING_STRENGTH = 650,
            TOWERING_DEFENSE = 651,
            LUMINOUS_VITALITY = 652,
            CRITICAL_DAMAGE_REDUCTION_METAMORPHI = 653,
            MAJOR_IMPREGNABILITY = 654,
            SIGIL_OF_PERSERVERANCE = 657,
            SIGIL_OF_PURITY_IX = 658,
            WEAVE_OF_ALCHEMY = 659,
            CLOAKED_IN_SKILL = 660,
            SHROUD_OF_DARKNESS_MAGIC = 661,
            SHROUD_OF_DARKNESS_MELEE = 662,
            SHROUD_OF_DARKNESS_MISSILE = 663,
            DIRTY_FIGHTING_INEPTITUDE = 664,
            DIRTY_FIGHTING_MASTERY = 665,
            MINOR_DIRTY_FIGHTING_PROWESS = 666,
            DUAL_WIELD_INEPTITUDE = 667,
            DUAL_WIELD_MASTERY = 668,
            MINOR_DUAL_WIELD_APTITUDE = 669,
            RECKLESSNESS_INEPTITUDE = 670,
            RECKLESSNESS_MASTERY = 671,
            MINOR_RECKLESSNESS_PROWESS = 672,
            SHIELD_INEPTITUDE = 673,
            SHIELD_MASTERY = 674,
            MINOR_SHIELD_APTITUDE = 675,
            SNEAK_ATTACK_INEPTITUDE = 676,
            SNEAK_ATTACK_MASTERY = 677,
            MINOR_SNEAK_ATTACK_PROWESS = 678,
            PRODIGAL_DIRTY_FIGHTING_MASTERY = 679,
            PRODIGAL_DUAL_WIELD_MASTERY = 680,
            PRODIGAL_RECKLESSNESS_MASTERY = 681,
            PRODIGAL_SHIELD_MASTERY = 682,
            PRODIGAL_SNEAK_ATTACK_MASTERY = 683,
            BLINDING_ASSAULT = 684,
            BLEEDING_ASSAULT = 685,
            UNBALANCING_ASSAULT = 686,
            TRAUMATIC_ASSAULT = 687,
            NOVICE_SOLDIERS_DIRTY_FIGHTING_APTITUDE = 688,
            NOVICE_SOLDIERS_DUAL_WIELD_APTITUDE = 689,
            NOVICE_SOLDIERS_RECKLESSNESS_APTITUDE = 690,
            NOVICE_SOLDIERS_SHIELD_APTITUDE = 691,
            NOVICE_SOLDIERS_SNEAK_ATTACK_APTITUDE = 692,
            VIGOR_OF_MHOIRE = 693,
            RARE_ARMOR_DAMAGE_BOOST = 694,
            AURA_OF_SPIRIT_DRINKER = 695,
            SUMMONING_MASTERY = 696,
            SUMMONING_INEPTITUDE = 697,
            EPIC_SUMMONING_PROWESS = 698,
            NOVICE_INVOKERS_SUMMONING_APTITUDE = 699,
            HONEYED_LIFE_MEAD = 700,
            HONEYED_MANA_MEAD = 701,
            HONEYED_VIGOR_MEAD = 702,
            TWISTING_WOUNDS = 703,
            PARAGONS_ENDURANCE = 704,
            PARAGONS_MANA = 705,
            PARAGONS_STAMINA = 706,
            PARAGONS_DIRTY_FIGHTING_MASTERY = 707,
            PARAGONS_DUAL_WIELD_MASTERY = 708,
            PARAGONS_RECKLESSNESS_MASTERY = 709,
            PARAGONS_SNEAK_ATTACK_MASTERY = 710,
            PARAGONS_DAMAGE_BOOST = 711,
            PARAGONS_DAMAGE_REDUCTION = 712,
            PARAGONS_CRITICAL_BOOST = 713,
            PARAGONS_CRITICAL_DAMAGE_REDUCTION = 714,
            PARAGONS_LIGHT_WEAPON_MASTERY = 715,
            PARAGONS_FINESSE_WEAPON_MASTERY = 716,
            PARAGONS_HEAVY_WEAPON_MASTERY = 717,
            PARAGONS_WAR_MAGIC_MASTERY = 718,
            PARAGONS_LIFE_MAGIC_MASTERY = 719,
            PARAGONS_VOID_MAGIC_MASTERY = 720,
            PARAGONS_MISSILE_WEAPON_MASTERY = 721,
            PARAGONS_STRENGTH = 722,
            PARAGONS_COORDINATION = 723,
            PARAGON_QUICKNESS = 724,
            PARAGONS_FOCUS = 725,
            PARAGONS_WILLPOWER = 726,
            PARAGONS_TWO_HANDED_COMBAT_MASTERY = 727
        }
        
        public static int GetNextSpellIdToRefresh(List<string> spellNames) {
            foreach (var name in spellNames) {
                if (DoesSpellNeedRefresh(name)) {
                    return GetIdFromName(name);
                }
            }

            return 0;
        }

        public static bool DoesAnySpellNeedRefresh(List<string> spellNames) {
            foreach (var name in spellNames) {
                if (DoesSpellNeedRefresh(name)) {
                    return true;
                }
            }

            return false;
        }

        public static bool DoesSpellNeedRefresh(string spellName) {
            try {
                int spellId = GetIdFromName(spellName);
                var enchantments = CoreManager.Current.CharacterFilter.Enchantments;
                
                if (!CoreManager.Current.CharacterFilter.SpellBook.Contains(spellId)) {
                    Util.WriteToChat(String.Format("I don't know this spell: {0} ({1})", spellName, spellId));
                    return false;
                }

                foreach (var enchantment in enchantments) {
                    string enchantmentName = Spells.GetNameFromId(enchantment.SpellId);

                    // TODO: Don't buff if equipment contains the same buff
                    if (enchantment.Duration == -1) continue;

                    if (enchantmentName == spellName) {
                        if (enchantment.Expires - DateTime.Now < TimeSpan.FromMinutes(Config.Bot.BuffRefreshTime.Value)) {
                            return true;
                        }

                        return false;
                    }
                }

                return true;
            }
            catch (Exception e) { Util.LogException(e); }

            return false;
        }

        public static bool CanCast(Decal.Filters.Spell spell) {
            // is this spell known?
            if (!CoreManager.Current.CharacterFilter.SpellBook.Contains(spell.Id)) {
                return false;
            }

            var currentSkill = 0;

            switch (spell.School.ToString()) {
                case "Creature Enchantment":
                    if (!CoreManager.Current.CharacterFilter.Skills[CharFilterSkillType.CreatureEnchantment].Known) return false;
                    currentSkill = CoreManager.Current.CharacterFilter.EffectiveSkill[CharFilterSkillType.CreatureEnchantment];
                    break;

                case "Life Magic":
                    if (!CoreManager.Current.CharacterFilter.Skills[CharFilterSkillType.LifeMagic].Known) return false;
                    currentSkill = CoreManager.Current.CharacterFilter.EffectiveSkill[CharFilterSkillType.LifeMagic];
                    break;

                case "Item Enchantment":
                    if (!CoreManager.Current.CharacterFilter.Skills[CharFilterSkillType.ItemEnchantment].Known) return false;
                    currentSkill = CoreManager.Current.CharacterFilter.EffectiveSkill[CharFilterSkillType.ItemEnchantment];
                    break;

                case "War Magic":
                    if (!CoreManager.Current.CharacterFilter.Skills[CharFilterSkillType.WarMagic].Known) return false;
                    currentSkill = CoreManager.Current.CharacterFilter.EffectiveSkill[CharFilterSkillType.WarMagic];
                    break;
            }

            // enough skill?
            if (currentSkill < spell.Difficulty) return false;
            
            // TODO: components

            return true;
        }

        public static Spell GetBestStaminaRecoverySpell(bool isSelf) {
            return GetBestKnownSpellFrom(STAMINA_RECOVERY_SPELLS);
        }

        public static Spell GetBestManaRecoverySpell(bool isSelf) {
            return GetBestKnownSpellFrom(MANA_RECOVERY_SPELLS);
        }

        private static Spell GetBestKnownSpellFrom(string[] spells) {
            Spell bestSpell = null;
            try {
                FileService fs = CoreManager.Current.Filter<FileService>();

                foreach (var spellName in spells) {
                    var spell = fs.SpellTable.GetByName(spellName);

                    if (spell == null) {
                        Util.WriteToChat("Could not find spell: " + spellName);
                        continue;
                    }

                    if (CanCast(spell) && (bestSpell == null || (spell.Difficulty > bestSpell.Difficulty))) {
                        bestSpell = spell;
                    }
                }
            }
            catch (Exception ex) { Util.LogException(ex); }

            return bestSpell;
        }

        public static Spell GetBestKnownSpellByClass(SpellClass spellClass, bool isSelf) {
            Spell bestSpell = null;
            FileService fs = CoreManager.Current.Filter<FileService>();

            foreach (var spellId in CoreManager.Current.CharacterFilter.SpellBook) {
                var spell = fs.SpellTable.GetById(spellId);

                if (isSelf && !spell.IsUntargetted) continue;

                if (spell.Family == (int)spellClass && CanCast(spell)) {
                    if (bestSpell == null) {
                        bestSpell = spell;
                    }
                    else if (spell.Difficulty > bestSpell.Difficulty){
                        bestSpell = spell;
                    }
                }
            }

            return bestSpell;
        }

        public static string GetNameFromId(int id) {
            FileService fs = CoreManager.Current.Filter<FileService>();
            var spell = fs.SpellTable.GetById(id);

            if (spell != null) {
                return spell.Name;
            }

            return null;
        }

        public static int GetIdFromName(string name) {
            FileService fs = CoreManager.Current.Filter<FileService>();
            var spell = fs.SpellTable.GetByName(name);

            if (spell != null) {
                return spell.Id;
            }

            return 0;
        }
    }
}

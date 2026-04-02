using System.Collections.Generic;
using UnityEngine;

public enum LetterState
{
    Empty,
    Wrong,      // gray  - letter not in word
    Misplaced,  // yellow - letter in word but wrong position
    Correct     // green  - letter in correct position
}

public class WordlePuzzle
{
    public const int WordLength = 5;
    public const int MaxAttempts = 6;

    private string targetWord;
    private List<string> guesses = new List<string>();
    private bool isSolved;
    private bool isFailed;

    private static readonly string[] wordList = new string[]
    {
        "FLAME", "STONE", "PEARL", "CRANE", "SWORD",
        "CLOUD", "TIGER", "RIVER", "MOUNT", "DREAM",
        "STAFF", "GHOST", "QUEEN", "FEAST", "HEART",
        "MONKS", "DEMON", "STORM", "SPINE", "GUARD",
        "ROYAL", "MAGIC", "SCALE", "BRAVE", "TOWER",
        "LIGHT", "SHADE", "EARTH", "PLANT", "BLADE",
        "NIGHT", "FROST", "QUEST", "HORSE", "RIDGE",
        "CHARM", "SAINT", "LOTUS", "GRAIN", "REIGN",
        "POWER", "ROBES", "DRINK", "FORGE", "TRAIL",
        "BEAST", "CREST", "TEETH", "BLOOD", "HONOR"
    };

    // ~1500 valid 5-letter guesses (includes target words + common English words)
    private static readonly HashSet<string> validGuesses = new HashSet<string>(wordList)
    {
        "ABACK", "ABATE", "ABBEY", "ABBOT", "ABHOR",
        "ABIDE", "ABLED", "ABODE", "ABORT", "ABOUT",
        "ABOVE", "ABUSE", "ABYSS", "ACIDS", "ACORN",
        "ACRES", "ACTED", "ACTOR", "ACUTE", "ADAGE",
        "ADAPT", "ADDED", "ADEPT", "ADMIN", "ADMIT",
        "ADOBE", "ADOPT", "ADULT", "AFTER", "AGAIN",
        "AGENT", "AGGRO", "AGILE", "AGING", "AGLOW",
        "AGONY", "AGREE", "AHEAD", "AIDED", "AIMED",
        "AIRED", "AISLE", "ALARM", "ALBUM", "ALERT",
        "ALGAE", "ALIBI", "ALIEN", "ALIGN", "ALIKE",
        "ALIVE", "ALLAY", "ALLEY", "ALLOT", "ALLOW",
        "ALLOY", "ALOFT", "ALONE", "ALONG", "ALOOF",
        "ALOUD", "ALPHA", "ALTAR", "ALTER", "AMASS",
        "AMAZE", "AMBER", "AMBLE", "AMEND", "AMINE",
        "AMINO", "AMISS", "AMONG", "AMPLE", "AMPLY",
        "AMUSE", "ANGEL", "ANGER", "ANGLE", "ANGRY",
        "ANGST", "ANIME", "ANKLE", "ANNEX", "ANNOY",
        "ANTIC", "ANVIL", "AORTA", "APART", "APPLE",
        "APPLY", "APRON", "APTLY", "ARBOR", "ARDOR",
        "ARENA", "ARGUE", "ARISE", "ARMOR", "AROMA",
        "AROSE", "ARRAY", "ARROW", "ARSON", "ASHEN",
        "ASIDE", "ASKED", "ASSET", "ATLAS", "ATONE",
        "ATTIC", "AUDIO", "AUDIT", "AUGUR", "AVAIL",
        "AVERT", "AVIAN", "AVOID", "AWAIT", "AWAKE",
        "AWARD", "AWARE", "AWFUL", "AWOKE", "AXIAL",
        "AXIOM", "AZURE", "BACON", "BADGE", "BADLY",
        "BAGEL", "BAGGY", "BAKER", "BALLS", "BANDS",
        "BANKS", "BARON", "BASED", "BASES", "BASIC",
        "BASIN", "BASIS", "BATCH", "BATHE", "BATON",
        "BEACH", "BEADS", "BEARD", "BEARS", "BEATS",
        "BEGAN", "BEGIN", "BEING", "BELLS", "BELLY",
        "BELOW", "BENCH", "BERRY", "BIBLE", "BIKES",
        "BILLS", "BIRDS", "BIRTH", "BLACK", "BLAME",
        "BLAND", "BLANK", "BLAST", "BLAZE", "BLEAK",
        "BLEAT", "BLEED", "BLEND", "BLESS", "BLIND",
        "BLINK", "BLISS", "BLITZ", "BLOAT", "BLOCK",
        "BLOKE", "BLOND", "BLOOM", "BLOWN", "BLUES",
        "BLUFF", "BLUNT", "BLURT", "BLUSH", "BOARD",
        "BOAST", "BOATS", "BOLTS", "BOMBS", "BONDS",
        "BONES", "BONUS", "BOOKS", "BOOST", "BOOTH",
        "BOOTS", "BOOTY", "BOOZE", "BORED", "BOSSY",
        "BOTCH", "BOUND", "BOXER", "BRACE", "BRAID",
        "BRAIN", "BRAND", "BRASH", "BRASS", "BRAWL",
        "BREAD", "BREAK", "BREED", "BRICK", "BRIDE",
        "BRIEF", "BRINE", "BRING", "BRINK", "BRISK",
        "BROAD", "BROIL", "BROKE", "BROOD", "BROOK",
        "BROTH", "BROWN", "BRUSH", "BRUTE", "BUDDY",
        "BUILD", "BUILT", "BULGE", "BULKY", "BULLS",
        "BULLY", "BUNCH", "BUNNY", "BURST", "BUSES",
        "BUYER", "CABIN", "CABLE", "CADET", "CALLS",
        "CAMEL", "CAMPS", "CANAL", "CANDY", "CARDS",
        "CARGO", "CAROL", "CARRY", "CARVE", "CASES",
        "CATCH", "CATER", "CAUSE", "CAVES", "CEASE",
        "CEDAR", "CELLS", "CHAIN", "CHAIR", "CHALK",
        "CHAMP", "CHANT", "CHAOS", "CHAPS", "CHARD",
        "CHASE", "CHEAP", "CHEAT", "CHECK", "CHEEK",
        "CHEER", "CHESS", "CHEST", "CHICK", "CHIEF",
        "CHILD", "CHILI", "CHILL", "CHINA", "CHIRP",
        "CHOIR", "CHORD", "CHORE", "CHOSE", "CHUNK",
        "CHURN", "CIDER", "CIGAR", "CINCH", "CITED",
        "CIVIC", "CIVIL", "CLAIM", "CLAMP", "CLANG",
        "CLANK", "CLASH", "CLASP", "CLASS", "CLAWS",
        "CLEAN", "CLEAR", "CLERK", "CLICK", "CLIFF",
        "CLIMB", "CLING", "CLIPS", "CLOAK", "CLOCK",
        "CLONE", "CLOSE", "CLOTH", "CLOUT", "CLOWN",
        "CLUBS", "CLUCK", "CLUED", "CLUMP", "CLUNG",
        "COACH", "COAST", "COBRA", "COCOA", "COILS",
        "COINS", "COLOR", "COMET", "COMIC", "COMMA",
        "CONDO", "CONIC", "CORAL", "CORNY", "COSTS",
        "COUCH", "COUGH", "COULD", "COUNT", "COUPE",
        "COURT", "COVER", "COVET", "CRACK", "CRAFT",
        "CRAMP", "CRASH", "CRATE", "CRAVE", "CRAWL",
        "CRAZE", "CRAZY", "CREAK", "CREAM", "CREST",
        "CRISP", "CROPS", "CROSS", "CROWD", "CROWN",
        "CRUDE", "CRUEL", "CRUSH", "CRUST", "CUBIC",
        "CURSE", "CURVE", "CURVY", "CYCLE", "CYNIC",
        "DADDY", "DAILY", "DAIRY", "DAISY", "DANCE",
        "DARTS", "DATED", "DATES", "DATUM", "DEALT",
        "DEATH", "DEBIT", "DEBUG", "DEBUT", "DECAL",
        "DECAY", "DECKS", "DECOR", "DECOY", "DECRY",
        "DEITY", "DELAY", "DELTA", "DELVE", "DENSE",
        "DEPOT", "DEPTH", "DERBY", "DETER", "DETOX",
        "DEUCE", "DEVIL", "DIARY", "DIGIT", "DINER",
        "DIRTY", "DISCO", "DITCH", "DITTO", "DIZZY",
        "DODGE", "DOGMA", "DOING", "DOLLS", "DONOR",
        "DONUT", "DOORS", "DOUBT", "DOUGH", "DOWNS",
        "DOZED", "DOZEN", "DRAFT", "DRAIN", "DRAKE",
        "DRAMA", "DRANK", "DRAPE", "DRAWL", "DRAWN",
        "DRAWS", "DREAD", "DRESS", "DRIED", "DRIFT",
        "DRILL", "DRIVE", "DROIT", "DROLL", "DRONE",
        "DROOL", "DROOP", "DROPS", "DROSS", "DROVE",
        "DROWN", "DRUGS", "DRUMS", "DRUNK", "DRYER",
        "DRYLY", "DUCAL", "DUCKS", "DUETS", "DUMMY",
        "DUMPS", "DUNCE", "DUNES", "DUSTY", "DUTCH",
        "DWARF", "DWELL", "DYING", "EAGER", "EAGLE",
        "EARLY", "EARNS", "EATEN", "EATER", "EBONY",
        "EDGES", "EDICT", "EIGHT", "EJECT", "ELBOW",
        "ELDER", "ELECT", "ELITE", "ELOPE", "ELUDE",
        "EMAIL", "EMBER", "EMCEE", "EMITS", "EMPTY",
        "ENDED", "ENEMY", "ENJOY", "ENTER", "ENTRY",
        "ENVOY", "EPOCH", "EQUAL", "EQUIP", "ERASE",
        "ERODE", "ERROR", "ESSAY", "ETHIC", "EVADE",
        "EVENT", "EVERY", "EVICT", "EVOKE", "EXACT",
        "EXALT", "EXAMS", "EXCEL", "EXERT", "EXILE",
        "EXIST", "EXPAT", "EXPEL", "EXTRA", "EXUDE",
        "EXULT", "FABLE", "FACES", "FACET", "FACTS",
        "FADED", "FAILS", "FAINT", "FAIRY", "FAITH",
        "FALSE", "FAMED", "FANCY", "FANGS", "FARCE",
        "FARMS", "FATAL", "FATTY", "FAULT", "FAUNA",
        "FAVOR", "FEAST", "FEELS", "FEIGN", "FEINT",
        "FENCE", "FERRY", "FETAL", "FETCH", "FEVER",
        "FEWER", "FIBER", "FIELD", "FIEND", "FIERY",
        "FIFTH", "FIFTY", "FIGHT", "FILET", "FILLS",
        "FILMS", "FILTH", "FINAL", "FINCH", "FINDS",
        "FINED", "FINER", "FIRED", "FIRES", "FIRST",
        "FIXED", "FIXER", "FLAGS", "FLAIR", "FLAKE",
        "FLAKY", "FLANK", "FLARE", "FLASH", "FLASK",
        "FLATS", "FLAWS", "FLEET", "FLESH", "FLICK",
        "FLING", "FLINT", "FLIPS", "FLIRT", "FLOAT",
        "FLOCK", "FLOOD", "FLOOR", "FLOPS", "FLORA",
        "FLOSS", "FLOUR", "FLOWS", "FLUID", "FLUKE",
        "FLUNG", "FLUNK", "FLUSH", "FLUTE", "FOAMY",
        "FOCAL", "FOCUS", "FOGGY", "FOILS", "FOLDS",
        "FOLKS", "FOLLY", "FONTS", "FOODS", "FORAY",
        "FORCE", "FORGO", "FORMS", "FORTE", "FORTH",
        "FORTY", "FORUM", "FOSSIL","FOUND", "FOXES",
        "FOYER", "FRAIL", "FRAME", "FRANK", "FRAUD",
        "FREAK", "FREED", "FRESH", "FRIAR", "FRIED",
        "FRIES", "FRILL", "FRISK", "FRITZ", "FROCK",
        "FRONT", "FROZE", "FRUIT", "FRYER", "FUDGE",
        "FUELS", "FUGUE", "FULLY", "FUNDS", "FUNGI",
        "FUNKY", "FUNNY", "FURRY", "FUSED", "FUSSY",
        "FUZZY", "GAINS", "GAMMA", "GANGS", "GASES",
        "GATES", "GAUGE", "GAUNT", "GAUZE", "GAVEL",
        "GAWKY", "GAZED", "GEARS", "GENRE", "GHOST",
        "GIANT", "GIFTS", "GIVEN", "GIVER", "GIVES",
        "GIZMO", "GLAND", "GLARE", "GLASS", "GLAZE",
        "GLEAM", "GLEAN", "GIDDY", "GIRLS", "GIRTH",
        "GLOBE", "GLOOM", "GLORY", "GLOSS", "GLOVE",
        "GNASH", "GNOME", "GOATS", "GODLY", "GOING",
        "GOLLY", "GOODS", "GOOFY", "GOOSE", "GORGE",
        "GOTTA", "GOUGE", "GOURD", "GRACE", "GRADE",
        "GRAFT", "GRAND", "GRANT", "GRAPE", "GRAPH",
        "GRASP", "GRASS", "GRATE", "GRAVE", "GRAVY",
        "GRAZE", "GREAT", "GREED", "GREEN", "GREET",
        "GRIEF", "GRILL", "GRIME", "GRIMY", "GRIND",
        "GRIPE", "GRIPS", "GRIST", "GROAN", "GROIN",
        "GROOM", "GROPE", "GROSS", "GROUP", "GROUT",
        "GROVE", "GROWL", "GROWN", "GROWS", "GRUEL",
        "GRUFF", "GRUNT", "GUARD", "GUAVA", "GUESS",
        "GUEST", "GUIDE", "GUILD", "GUILT", "GUISE",
        "GULCH", "GULLS", "GUMMY", "GUSTO", "GUSTY",
        "GYPSY", "HABIT", "HALLS", "HANDS", "HANDY",
        "HANGS", "HAPPY", "HARDY", "HASTE", "HASTY",
        "HATCH", "HAUNT", "HAVEN", "HAVOC", "HEADS",
        "HEALS", "HEARD", "HEATH", "HEAVE", "HEAVY",
        "HEDGE", "HEELS", "HEFTY", "HEIST", "HELLO",
        "HELPS", "HENCE", "HERBS", "HERDS", "HERON",
        "HILLY", "HINGE", "HIPPO", "HIRED", "HITCH",
        "HOIST", "HOLDS", "HOLES", "HOLLY", "HOMER",
        "HOMES", "HONEY", "HOOKS", "HOPED", "HORNS",
        "HOTEL", "HOUND", "HOURS", "HOUSE", "HOVER",
        "HOWDY", "HUMAN", "HUMID", "HUMOR", "HUMPS",
        "HURRY", "HYENA", "HYMNS", "HYPER", "ICING",
        "IDEAL", "IDEAS", "IMAGE", "IMAGO", "IMPLY",
        "INBOX", "INCUR", "INDEX", "INDIE", "INEPT",
        "INERT", "INFER", "INGOT", "INNER", "INPUT",
        "INTER", "INTRO", "IRATE", "IRONY", "ISSUE",
        "IVORY", "JAPAN", "JAUNT", "JAZZY", "JEANS",
        "JELLY", "JEWEL", "JIFFY", "JIMMY", "JOINS",
        "JOINT", "JOKER", "JOLLY", "JOUST", "JUDGE",
        "JUICE", "JUICY", "JUMBO", "JUMPS", "JUMPY",
        "JUROR", "KARMA", "KAYAK", "KEBAB", "KEEPS",
        "KICKS", "KINGS", "KIOSK", "KNACK", "KNEAD",
        "KNEEL", "KNELT", "KNIFE", "KNOBS", "KNOCK",
        "KNOLL", "KNOTS", "KNOWN", "KNOWS", "KUDOS",
        "LABEL", "LABOR", "LACED", "LADEN", "LADLE",
        "LAGER", "LAKES", "LAMBS", "LAMPS", "LANCE",
        "LANDS", "LANES", "LAPSE", "LARGE", "LASER",
        "LATCH", "LATER", "LATHE", "LATIN", "LAUGH",
        "LATCH", "LATTE", "LAYER", "LEADS", "LEAFY",
        "LEAKY", "LEAPS", "LEARN", "LEASE", "LEASH",
        "LEAST", "LEAVE", "LEDGE", "LEGAL", "LEMON",
        "LEVEL", "LEVER", "LIBEL", "LIFTS", "LIKEN",
        "LILAC", "LIMBO", "LIMBS", "LIMES", "LIMIT",
        "LINED", "LINEN", "LINER", "LINES", "LINGO",
        "LINKS", "LIONS", "LISTS", "LITER", "LIVED",
        "LIVEN", "LIVER", "LIVES", "LLAMA", "LOADS",
        "LOBBY", "LOCAL", "LODGE", "LOFTY", "LOGIC",
        "LOGIN", "LOOKS", "LOOPS", "LOOSE", "LORDS",
        "LORRY", "LOSSY", "LOVED", "LOVER", "LOWER",
        "LOYAL", "LUCID", "LUCKY", "LUMEN", "LUMPS",
        "LUMPY", "LUNAR", "LUNCH", "LUNGS", "LURCH",
        "LUSTY", "LYING", "LYRIC", "MACHO", "MACRO",
        "MAFIA", "MAJOR", "MAKER", "MAKES", "MALES",
        "MALLS", "MANGA", "MANGO", "MANGE", "MANIA",
        "MANOR", "MAPLE", "MARCH", "MARKS", "MARSH",
        "MASKS", "MASON", "MATCH", "MATES", "MAXIM",
        "MAYBE", "MAYOR", "MEALS", "MEANS", "MEANT",
        "MEDAL", "MEDIA", "MELEE", "MELON", "MERCY",
        "MERGE", "MERIT", "MERRY", "MESSY", "METAL",
        "METER", "MIDST", "MIGHT", "MILES", "MILLS",
        "MINCE", "MINDS", "MINED", "MINER", "MINES",
        "MINOR", "MINUS", "MIRTH", "MISER", "MISTY",
        "MITER", "MIXED", "MIXER", "MOANS", "MOCHA",
        "MODEL", "MODEM", "MODES", "MOGUL", "MOIST",
        "MOLAR", "MOLDY", "MONEY", "MONTH", "MOODS",
        "MOODY", "MOOSE", "MORAL", "MORPH", "MOSSY",
        "MOTEL", "MOTHS", "MOTOR", "MOTTO", "MOULD",
        "MOUND", "MOUSE", "MOUTH", "MOVED", "MOVER",
        "MOVES", "MOVIE", "MUCKY", "MUDDY", "MUFFIN",
        "MULCH", "MULTI", "MUMMY", "MUNCH", "MURAL",
        "MURKY", "MUSCLE","MUSIC", "MUSTY", "MYTHS",
        "NACHO", "NAIVE", "NAMED", "NAMES", "NANNY",
        "NASAL", "NASTY", "NAVAL", "NAVEL", "NEEDS",
        "NERVE", "NEVER", "NEWLY", "NEXUS", "NICHE",
        "NIGHT", "NINJA", "NOBLE", "NODES", "NOISE",
        "NOMAD", "NORMS", "NORTH", "NOTCH", "NOTED",
        "NOTES", "NOVEL", "NUDGE", "NURSE", "NUTTY",
        "NYLON", "OAKEN", "OASIS", "OCCUR", "OCEAN",
        "ODDLY", "ODORS", "OFFER", "OFTEN", "OLDEN",
        "OLIVE", "OMEGA", "ONSET", "OPERA", "OPTIC",
        "ORBIT", "ORDER", "ORGAN", "OTHER", "OTTER",
        "OUGHT", "OUNCE", "OUTER", "OUTDO", "OUTED",
        "OUTWIT","OVARY", "OVERT", "OWING", "OWNER",
        "OXIDE", "OZONE", "PACED", "PACKS", "PADDY",
        "PAGES", "PAINS", "PAINT", "PAIRS", "PALMS",
        "PANDA", "PANEL", "PANIC", "PANSY", "PANTS",
        "PAPER", "PARKS", "PARTS", "PARTY", "PASTA",
        "PASTE", "PATCH", "PATHS", "PATIO", "PAUSE",
        "PAVED", "PAYER", "PEACE", "PEACH", "PEAKS",
        "PEDAL", "PEEKS", "PEELS", "PENNY", "PERCH",
        "PERIL", "PERKS", "PERKY", "PESTO", "PETTY",
        "PHASE", "PHONE", "PHOTO", "PIANO", "PICKS",
        "PICKY", "PIECE", "PIETY", "PIGGY", "PILED",
        "PILES", "PILLS", "PILOT", "PINCH", "PINES",
        "PIPES", "PITCH", "PIXEL", "PIXIE", "PIZZA",
        "PLACE", "PLAID", "PLAIN", "PLANE", "PLANK",
        "PLANS", "PLATE", "PLAYS", "PLAZA", "PLEAD",
        "PLEAT", "PLIED", "PLIER", "PLOTS", "PLOWS",
        "PLUCK", "PLUGS", "PLUMB", "PLUME", "PLUMP",
        "PLUMS", "PLUNGE","PLUNK", "PLUSH", "POINT",
        "POISE", "POKER", "POLAR", "POLLS", "POLYP",
        "PONDS", "POOLS", "PORCH", "PORES", "PORTS",
        "POSED", "POSER", "POSIT", "POSSE", "POSTS",
        "POUCH", "POUND", "POWER", "PRANK", "PRAWN",
        "PREACH","PRESS", "PRICE", "PRIDE", "PRIED",
        "PRIEST","PRIME", "PRINT", "PRIOR", "PRISM",
        "PRIVY", "PRIZE", "PROBE", "PROMO", "PRONE",
        "PRONG", "PROOF", "PROPS", "PROSE", "PROUD",
        "PROVE", "PROWL", "PRUDE", "PRUNE", "PSALM",
        "PULSE", "PUMPS", "PUNCH", "PUPIL", "PUPPY",
        "PURGE", "PURSE", "PUSHY", "PUTTY", "QUACK",
        "QUAIL", "QUALM", "QUART", "QUASI", "QUEEN",
        "QUEER", "QUERY", "QUEUE", "QUICK", "QUIET",
        "QUILL", "GUILT", "QUIRK", "QUITE", "QUOTA",
        "QUOTE", "RABBI", "RACES", "RADAR", "RADIO",
        "RAIDS", "RAILS", "RAINY", "RAISE", "RALLY",
        "RANCH", "RANGE", "RANKS", "RAPID", "RATED",
        "RATES", "RATIO", "RAVEN", "REACH", "READS",
        "READY", "REALM", "REBEL", "RECAP", "RECON",
        "RECUR", "REEDS", "REFER", "REGAL", "REIGN",
        "REINS", "RELAX", "RELAY", "RELIC", "REMIT",
        "RENEW", "REPAY", "REPEL", "REPLY", "RESET",
        "RESIN", "RETRO", "REUSE", "REVEL", "RIDER",
        "RIDES", "RIFLE", "RIGHT", "RIGID", "RIGOR",
        "RINDS", "RINGS", "RINSE", "RIOTS", "RIPEN",
        "RISEN", "RISES", "RISKY", "RITES", "RIVAL",
        "ROADS", "ROAST", "ROBOT", "ROCKS", "ROCKY",
        "ROGUE", "ROLES", "ROLLS", "ROMAN", "ROOMS",
        "ROOTS", "ROPES", "ROSES", "ROTOR", "ROUGH",
        "ROUND", "ROUTE", "ROVER", "ROWDY", "ROYAL",
        "RUGBY", "RUINS", "RULED", "RULER", "RULES",
        "RUMBA", "RUMOR", "RUNGS", "RURAL", "RUSTY",
        "SADLY", "SAFER", "SAFES", "SAINT", "SALAD",
        "SALES", "SALON", "SALSA", "SALTY", "SALVE",
        "SANDS", "SANDY", "SAUCE", "SAUCY", "SAUNA",
        "SAVED", "SAVOR", "SAVVY", "SCALE", "SCALP",
        "SCAMS", "SCANT", "SCARE", "SCARF", "SCARY",
        "SCENE", "SCENT", "SCOPE", "SCOOP", "SCORE",
        "SCORN", "SCOUT", "SCOWL", "SCRAM", "SCRAP",
        "SCREW", "SCRUB", "SEAMS", "SEEDS", "SEIZE",
        "SENSE", "SEPIA", "SERVE", "SETUP", "SEVEN",
        "SEVER", "SHADE", "SHAFT", "SHAKY", "SHALL",
        "SHAME", "SHAPE", "SHARE", "SHARK", "SHARP",
        "SHAWL", "SHEAR", "SHEEN", "SHEEP", "SHEER",
        "SHEET", "SHELF", "SHELL", "SHIFT", "SHINE",
        "SHINY", "SHIPS", "SHIRE", "SHIRT", "SHOCK",
        "SHOES", "SHOOK", "SHOOT", "SHORE", "SHORT",
        "SHOTS", "SHOUT", "SHOVE", "SHOWN", "SHOWS",
        "SHRUB", "SHRUG", "SIDES", "SIEGE", "SIGHT",
        "SIGMA", "SIGNS", "SILKY", "SILLY", "SINCE",
        "SIREN", "SIXTY", "SIZED", "SIZES", "SKATE",
        "SKILL", "SKIMP", "SKINS", "SKIRT", "SKULL",
        "SLACK", "SLAIN", "SLANG", "SLANT", "SLATE",
        "SLEEK", "SLEEP", "SLEET", "SLEPT", "SLICE",
        "SLIDE", "SLIME", "SLIMY", "SLING", "SLINK",
        "SLOPE", "SLOTH", "SLUGS", "SLUMP", "SLUSH",
        "SMALL", "SMART", "SMASH", "SMEAR", "SMELL",
        "SMELT", "SMILE", "SMIRK", "SMITH", "SMOCK",
        "SMOKE", "SMOKY", "SNACK", "SNAIL", "SNAKE",
        "SNARE", "SNARL", "SNEAK", "SNEER", "SNIFF",
        "SNORE", "SNORT", "SNOUT", "SNOWY", "SOBER",
        "SOCKS", "SOLAR", "SOLID", "SOLVE", "SONGS",
        "SONIC", "SORRY", "SORTS", "SOULS", "SOUND",
        "SOUTH", "SPACE", "SPADE", "SPARE", "SPARK",
        "SPAWN", "SPEAK", "SPEAR", "SPECS", "SPEED",
        "SPELL", "SPEND", "SPENT", "SPICE", "SPICY",
        "SPIED", "SPIKE", "SPILL", "SPINE", "SPINY",
        "SPLIT", "SPOKE", "SPOON", "SPORT", "SPOTS",
        "SPRAY", "SPREE", "SPRIG", "SPUNK", "SQUAD",
        "SQUAT", "SQUID", "STACK", "STAFF", "STAGE",
        "STAIN", "STAIR", "STAKE", "STALE", "STALK",
        "STALL", "STAMP", "STAND", "STANK", "STAPH",
        "STARE", "STARK", "STARS", "START", "STASH",
        "STATE", "STAYS", "STEAK", "STEAL", "STEAM",
        "STEEL", "STEEP", "STEER", "STEMS", "STEPS",
        "STERN", "STIFF", "STILL", "STING", "STINK",
        "STINT", "STOCK", "STOKE", "STOLE", "STOMP",
        "STOOD", "STOOL", "STOOP", "STOPS", "STORE",
        "STORK", "STORY", "STOUT", "STOVE", "STRAY",
        "STRIP", "STRUM", "STRUT", "STUCK", "STUDY",
        "STUFF", "STUMP", "STUNG", "STUNK", "STUNT",
        "STYLE", "SUGAR", "SUITE", "SUITS", "SULKY",
        "SUNNY", "SUPER", "SURGE", "SUSHI", "SWAMP",
        "SWANS", "SWARM", "SWEAR", "SWEAT", "SWEEP",
        "SWEET", "SWELL", "SWEPT", "SWIFT", "SWINE",
        "SWING", "SWIPE", "SWIRL", "SWOOP", "SWORE",
        "SWORN", "SWUNG", "SYRUP", "TABLE", "TACIT",
        "TAINT", "TAKEN", "TALES", "TALKS", "TALLY",
        "TALON", "TANGO", "TANKS", "TAPER", "TAPES",
        "TARDY", "TASKS", "TASTE", "TASTY", "TAUNT",
        "TAXES", "TEACH", "TEAMS", "TEARS", "TEASE",
        "TEDDY", "TEENS", "TEMPO", "TENDS", "TENOR",
        "TENSE", "TENTH", "TENTS", "TEPID", "TERMS",
        "TESTS", "THANK", "THEFT", "THEIR", "THEME",
        "THERE", "THESE", "THICK", "THIEF", "THIGH",
        "THING", "THINK", "THIRD", "THORN", "THOSE",
        "THREE", "THREW", "THROW", "THUDS", "THUMB",
        "TIDAL", "TIERS", "TIGHT", "TILES", "TILTS",
        "TIMED", "TIMER", "TIMES", "TIMID", "TIPSY",
        "TIRED", "TITAN", "TITLE", "TOAST", "TODAY",
        "TOKEN", "TOLLS", "TONAL", "TONED", "TONES",
        "TONIC", "TOOLS", "TOOTH", "TOPAZ", "TOPIC",
        "TORCH", "TOTAL", "TOTEM", "TOUCH", "TOUGH",
        "TOURS", "TOWEL", "TOWNS", "TOXIC", "TRACE",
        "TRACK", "TRADE", "TRAIN", "TRAIT", "TRAMP",
        "TRAPS", "TRASH", "TRAWL", "TREAD", "TREAT",
        "TREES", "TREND", "TRIAD", "TRIAL", "TRIBE",
        "TRICK", "TRIED", "TRIES", "TRIMS", "TRIOS",
        "TRIPS", "TRITE", "TROLL", "TROOP", "TROPE",
        "TROUT", "TRUCE", "TRUCK", "TRULY", "TRUMP",
        "TRUNK", "TRUST", "TRUTH", "TUBES", "TULIP",
        "TUMOR", "TUNES", "TUNIC", "TURBO", "TURNS",
        "TUTOR", "TWEAK", "TWEED", "TWEET", "TWICE",
        "TWIGS", "TWINE", "TWIRL", "TWIST", "TYING",
        "TYPED", "TYPES", "UDDER", "ULTRA", "UNCLE",
        "UNCUT", "UNDER", "UNDUE", "UNDID", "UNFIT",
        "UNION", "UNITE", "UNITS", "UNITY", "UNLIT",
        "UNTIL", "UPPER", "UPSET", "URBAN", "URGED",
        "USAGE", "USERS", "USHER", "USING", "USUAL",
        "UTTER", "VAGUE", "VALID", "VALOR", "VALUE",
        "VALVE", "VAULT", "VEGAN", "VEILS", "VEINS",
        "VENOM", "VENUE", "VERGE", "VERSE", "VIDEO",
        "VIGOR", "VINYL", "VIOLA", "VIPER", "VIRAL",
        "VIRUS", "VISIT", "VISOR", "VISTA", "VITAL",
        "VIVID", "VOCAL", "VODKA", "VOGUE", "VOICE",
        "VOTER", "VOUCH", "VOWEL", "VULVA", "WACKY",
        "WADED", "WAGER", "WAGES", "WAGON", "WAIST",
        "WALKS", "WALLS", "WALTZ", "WANDS", "WANTS",
        "WARDS", "WARNS", "WASTE", "WATCH", "WATER",
        "WAVED", "WAVER", "WAVES", "WAXED", "WEARY",
        "WEAVE", "WEDGE", "WEEDS", "WEEKS", "WEIGH",
        "WEIRD", "WELLS", "WHALE", "WHEAT", "WHEEL",
        "WHERE", "WHICH", "WHILE", "WHINE", "WHIRL",
        "WHISK", "WHITE", "WHOLE", "WHOSE", "WIDEN",
        "WIDER", "WIDOW", "WIDTH", "WIELD", "WINDS",
        "WINDY", "WINES", "WINGS", "WIPED", "WIRED",
        "WIRES", "WITCH", "WIVES", "WOKEN", "WOMAN",
        "WOMEN", "WOODS", "WOODY", "WORDS", "WORKS",
        "WORLD", "WORMS", "WORRY", "WORSE", "WORST",
        "WORTH", "WOULD", "WOUND", "WRATH", "WRECK",
        "WRIST", "WRITE", "WRONG", "WROTE", "YACHT",
        "YEARN", "YEAST", "YIELD", "YOUNG", "YOURS",
        "YOUTH", "ZEBRA", "ZESTY", "ZONES"
    };

    public string TargetWord => targetWord;
    public IReadOnlyList<string> Guesses => guesses;
    public int CurrentAttempt => guesses.Count;
    public bool IsSolved => isSolved;
    public bool IsFailed => isFailed;
    public bool IsGameOver => isSolved || isFailed;

    public WordlePuzzle()
    {
        targetWord = wordList[Random.Range(0, wordList.Length)];
    }

    public WordlePuzzle(string word)
    {
        targetWord = word.ToUpper();
    }

    public bool IsValidGuess(string guess)
    {
        if (guess.Length != WordLength) return false;
        return validGuesses.Contains(guess.ToUpper());
    }

    public LetterState[] SubmitGuess(string guess)
    {
        guess = guess.ToUpper();
        if (guess.Length != WordLength) return null;

        guesses.Add(guess);

        LetterState[] result = EvaluateGuess(guess);

        if (guess == targetWord)
            isSolved = true;
        else if (guesses.Count >= MaxAttempts)
            isFailed = true;

        return result;
    }

    public LetterState[] EvaluateGuess(string guess)
    {
        LetterState[] result = new LetterState[WordLength];
        bool[] targetUsed = new bool[WordLength];
        bool[] guessUsed = new bool[WordLength];

        // First pass: mark correct letters
        for (int i = 0; i < WordLength; i++)
        {
            if (guess[i] == targetWord[i])
            {
                result[i] = LetterState.Correct;
                targetUsed[i] = true;
                guessUsed[i] = true;
            }
        }

        // Second pass: mark misplaced letters
        for (int i = 0; i < WordLength; i++)
        {
            if (guessUsed[i]) continue;

            for (int j = 0; j < WordLength; j++)
            {
                if (targetUsed[j]) continue;

                if (guess[i] == targetWord[j])
                {
                    result[i] = LetterState.Misplaced;
                    targetUsed[j] = true;
                    break;
                }
            }

            if (result[i] == LetterState.Empty)
                result[i] = LetterState.Wrong;
        }

        return result;
    }
}

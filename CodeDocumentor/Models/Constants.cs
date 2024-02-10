// For definitions of XML nodes see:
// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments see
// also https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags
using System.Collections.Generic;
using System.Linq;
using CodeDocumentor.Helper;

namespace CodeDocumentor.Vsix2022
{
    public static class Constants
    {
        public static string[] PLURAL_EXCLUSIONS { get; set; } = new[] { "connection" };

        public static string[] ADD_THE_ANYWAY_LIST { get; set; } = new[] { "does" };

        private static string[] AXUILLARY_VERB_WORD_LIST { get; } = new[] {"are", "was", "were", "been", "being", "have", "does",
                                                                    "has", "had", "having", "set", "get",
                                                                    "did", "can", "shall", "will", "may", "might", "must",
                                                                    "dare", "need", "used", "ought", "goes" };

        private static string[] TWO_LETTER_WORD_LIST { get; } = new[] { "an", "at", "be", "by", "do", "go", "if", "in", "is", "it", "me",
                                                                    "my", "no", "of", "on", "or", "so", "to", "up", "us", "we", "am", "as", "ax", "by", "do", "go", "he",
                                                                    "hi", "if", "in", "is", "it", "me", "my", "no", "of", "oh", "on", "or", "ox", "so", "to", "uh", "um",
                                                                    "up", "us", "we" };

        public static string[] LETTER_S_SUFFIX_EXCLUSION_FOR_PLURALIZER { get; } = new[] { "as", "is", "his", "has","yes", "its", "ass" };

        //This checks for a set of words together being passed in as one word to evaluate. These sets of words together would could as a verb
        //private static string[] PLURALIZATION_VERB_CONVERSIONS { get; set; } = new[] { "converts to", "checks if is", "checks if", };

        public static WordMap[] INTERNAL_WORD_MAPS { get; set; } = new[] {
            new WordMap { Word = "To", Translation = "Converts to" },
            new WordMap { Word = "Do", Translation = "Does" },
            new WordMap { Word = "Dto", Translation = "Data transfer object" },
            new WordMap { Word = "Is", Translation = "Checks if is", OnlyIfInFirstPositon = true },
            new WordMap { Word = "Ensure", Translation = "Checks if is", WordEvaluator = (translation, nextWord)=>{
                    if(!string.IsNullOrEmpty(nextWord) && Pluralizer.IsPlural(nextWord)){
                        return "Checks if";
                    }
                    return translation;
                }
            }
        };

        //public static WordMap[] PLURALIZE_CUSTOM_LIST { get; set; } = new[] {

        //};

        public static WordMap[] DEFAULT_WORD_MAPS { get; set; } = new[] {
            new WordMap { Word = "int", Translation = "integer" },
            new WordMap { Word = "Int32", Translation = "integer" },
            new WordMap { Word = "Int64", Translation = "integer" },
            new WordMap { Word = "OfList", Translation = "OfLists" },
            new WordMap { Word = "OfEnumerable", Translation = "OfLists" },
            new WordMap { Word = "IEnumerable", Translation = "List" },
            new WordMap { Word = "IList", Translation = "List" },
            new WordMap { Word = "IReadOnlyList", Translation = "Read Only List" },
            new WordMap { Word = "ICollection", Translation = "Collection" },
            new WordMap { Word = "OfCollection", Translation = "OfCollections" },
            new WordMap { Word = "IReadOnlyCollection", Translation = "Read Only Collection" },
            new WordMap { Word = "IReadOnlyDictionary", Translation = "Read Only Dictionary" }
        };

        //These should match the
        //public static string[] PLURALIZE_ANYWAY_LIST()
        //{
        //    return INTERNAL_WORD_MAPS.Select(s => s.Word.ToLowerInvariant()).ToArray();
        //}

        public static class DiagnosticIds
        {
            public const string CLASS_DIAGNOSTIC_ID = "CD1600";
            public const string CONSTRUCTOR_DIAGNOSTIC_ID = "CD1601";
            public const string ENUM_DIAGNOSTIC_ID = "CD1602";
            public const string FIELD_DIAGNOSTIC_ID = "CD1603";
            public const string FILE_DIAGNOSTIC_ID = "CD1607";
            public const string INTERFACE_DIAGNOSTIC_ID = "CD1604";
            public const string METHOD_DIAGNOSTIC_ID = "CD1605";
            public const string PROPERTY_DIAGNOSTIC_ID = "CD1606";
            public const string RECORD_DIAGNOSTIC_ID = "CD1608";
        }

        public static IEnumerable<string> GetInternalVerbCheckList()
        {
            var items = new List<string>();
            items.AddRange(INTERNAL_VERB_WORD_LIST);
            items.AddRange(AXUILLARY_VERB_WORD_LIST);
            items.AddRange(TWO_LETTER_WORD_LIST);
            //items.AddRange(PLURALIZATION_VERB_CONVERSIONS);
            return items;
        }

        private static string[] INTERNAL_VERB_WORD_LIST { get; set; } = new[] {
"accept",
"access",
"add",
"admire",
"admit",
"advise",
"afford",
"agree",
"alert",
"allow",
"amuse",
"analyse",
"analyze",
"announce",
"annoy",
"answer",
"apologise",
"appear",
"applaud",
"appreciate",
"approve",
"argue",
"arrange",
"arrest",
"arrive",
"ask",
"attach",
"attack",
"attempt",
"attend",
"attract",
"await",
"avoid",
"back",
"bake",
"balance",
"ban",
"bang",
"bare",
"bat",
"bathe",
"battle",
"be",
"beam",
"beg",
"behave",
"belong",
"bleach",
"bless",
"blind",
"blink",
"blot",
"blush",
"boast",
"boil",
"bolt",
"bomb",
"book",
"bore",
"borrow",
"bounce",
"bow",
"box",
"brake",
"branch",
"breathe",
"bruise",
"brush",
"bubble",
"build",
"bump",
"burn",
"bury",
"buzz",
"calculate",
"call",
"camp",
"can",
"care",
"carry",
"carve",
"cause",
"challenge",
"change",
"charge",
"chase",
"cheat",
"check",
"cheer",
"chew",
"choke",
"chop",
"claim",
"clap",
"clean",
"clear",
"clip",
"close",
"coach",
"coil",
"collect",
"colour",
"comb",
"command",
"communicate",
"compare",
"compete",
"compile",
"complain",
"complete",
"compute",
"concentrate",
"concern",
"confess",
"confuse",
"connect",
"consider",
"consist",
"contain",
"continue",
"convert",
"converts",
"copy",
"correct",
"cough",
"could",
"count",
"cover",
"crack",
"crash",
"crawl",
"cross",
"crush",
"cry",
"cure",
"curl",
"curve",
"cycle",
"dam",
"damage",
"dance",
"dare",
"debug",
"decay",
"deceive",
"decide",
"decorate",
"delay",
"delight",
"deliver",
"depend",
"describe",
"deserialize",
"desert",
"deserve",
"destroy",
"detect",
"develop",
"disagree",
"disappear",
"disapprove",
"disarm",
"discover",
"dislike",
"display",
"divide",
"done",
"double",
"doubt",
"drag",
"drain",
"dream",
"dress",
"drip",
"drop",
"drown",
"drum",
"dry",
"dust",
"earn",
"educate",
"embarrass",
"employ",
"empty",
"encourage",
"end",
"enjoy",
"ensure",
"enter",
"entertain",
"escape",
"examine",
"excite",
"excuse",
"execute",
"exercise",
"exist",
"expand",
"expect",
"explain",
"explode",
"extend",
"face",
"fade",
"fail",
"fancy",
"fasten",
"fax",
"fear",
"fence",
"fetch",
"file",
"fill",
"film",
"fire",
"fit",
"fix",
"flap",
"flash",
"float",
"flood",
"flow",
"flower",
"fold",
"follow",
"fool",
"force",
"form",
"format",
"found",
"frame",
"frighten",
"fry",
"gather",
"gaze",
"generate",
"glow",
"glue",
"grab",
"grate",
"grease",
"greet",
"grin",
"grip",
"groan",
"guarantee",
"guard",
"guess",
"guide",
"hammer",
"hand",
"handle",
"hang",
"happen",
"harass",
"harm",
"hate",
"haunt",
"head",
"heal",
"heap",
"heat",
"help",
"hook",
"hop",
"hope",
"hover",
"hug",
"hum",
"hunt",
"hurry",
"identify",
"ignore",
"imagine",
"impress",
"improve",
"include",
"increase",
"influence",
"inform",
"inject",
"injure",
"instruct",
"intend",
"interest",
"interfere",
"interrupt",
"introduce",
"invent",
"invite",
"irritate",
"itch",
"iterate",
"jail",
"jam",
"jog",
"join",
"joke",
"judge",
"juggle",
"jump",
"kick",
"kill",
"kiss",
"kneel",
"knit",
"knock",
"knot",
"label",
"land",
"last",
"laugh",
"launch",
"learn",
"level",
"license",
"lick",
"lie",
"lighten",
"like",
"list",
"listen",
"live",
"load",
"lock",
"log",
"long",
"look",
"love",
"make",
"man",
"manage",
"march",
"mark",
"marry",
"match",
"mate",
"matter",
"measure",
"meddle",
"melt",
"memorise",
"mend",
"mess up",
"milk",
"mine",
"miss",
"mix",
"moan",
"modify",
"moor",
"mourn",
"move",
"muddle",
"mug",
"multiply",
"murder",
"nail",
"name",
"need",
"nest",
"nod",
"note",
"notice",
"number",
"obey",
"object",
"observe",
"obtain",
"occur",
"offend",
"offer",
"open",
"order",
"output",
"overflow",
"owe",
"own",
"pack",
"paddle",
"paint",
"park",
"parse",
"part",
"pass",
"paste",
"pat",
"pause",
"peck",
"pedal",
"peel",
"peep",
"perform",
"permit",
"phone",
"pick",
"pinch",
"pine",
"place",
"plan",
"plant",
"play",
"please",
"plug",
"point",
"poke",
"polish",
"pop",
"possess",
"post",
"pour",
"pray",
"preach",
"precede",
"prefer",
"prepare",
"present",
"preserve",
"press",
"pretend",
"prevent",
"prick",
"print",
"produce",
"program",
"promise",
"protect",
"provide",
"publish",
"pull",
"pump",
"punch",
"puncture",
"punish",
"push",
"query",
"question",
"queue",
"race",
"radiate",
"rain",
"raise",
"reach",
"realise",
"receive",
"recognise",
"record",
"reduce",
"reflect",
"refuse",
"regret",
"reign",
"reject",
"rejoice",
"relax",
"release",
"rely",
"remain",
"remember",
"remind",
"remove",
"render",
"repair",
"repeat",
"replace",
"reply",
"report",
"reproduce",
"request",
"rescue",
"retire",
"retrieve",
"return",
"rhyme",
"rinse",
"risk",
"rob",
"rock",
"roll",
"rot",
"rub",
"ruin",
"rule",
"run",
"rush",
"sack",
"sail",
"satisfy",
"save",
"saw",
"scare",
"scatter",
"scold",
"scorch",
"scrape",
"scratch",
"scream",
"screw",
"scribble",
"scrub",
"seal",
"search",
"separate",
"serialize",
"serve",
"settle",
"shade",
"share",
"shave",
"shelter",
"shiver",
"shock",
"shop",
"should",
"show",
"shrug",
"sigh",
"sign",
"signal",
"sin",
"sip",
"ski",
"skip",
"slap",
"slip",
"slow",
"smash",
"smell",
"smile",
"smoke",
"snatch",
"sneeze",
"sniff",
"snore",
"snow",
"soak",
"soothe",
"sound",
"spare",
"spark",
"sparkle",
"spell",
"spill",
"spoil",
"spot",
"spray",
"sprout",
"squash",
"squeak",
"squeal",
"squeeze",
"stain",
"stamp",
"stare",
"start",
"stay",
"steer",
"step",
"stir",
"stitch",
"stop",
"store",
"strap",
"strengthen",
"stretch",
"strip",
"stroke",
"stuff",
"subtract",
"succeed",
"suck",
"suffer",
"suggest",
"suit",
"supply",
"support",
"suppose",
"surprise",
"surround",
"suspect",
"suspend",
"switch",
"talk",
"tame",
"tap",
"taste",
"tease",
"telephone",
"tempt",
"terrify",
"test",
"thank",
"thaw",
"tick",
"tickle",
"tie",
"time",
"tip",
"tire",
"to",
"touch",
"tour",
"tow",
"trace",
"trade",
"train",
"transform",
"transmit",
"transport",
"trap",
"travel",
"treat",
"tremble",
"trick",
"trip",
"trot",
"trouble",
"trust",
"try",
"tug",
"tumble",
"turn",
"twist",
"type",
"undress",
"unfasten",
"unite",
"unlock",
"unpack",
"untidy",
"update",
"use",
"validate",
"vanish",
"visit",
"wail",
"wait",
"walk",
"wander",
"want",
"warm",
"warn",
"wash",
"waste",
"watch",
"water",
"wave",
"weigh",
"welcome",
"whine",
"whip",
"whirl",
"whisper",
"whistle",
"why",
"wink",
"wipe",
"wish",
"wobble",
"wonder",
"work",
"worry",
"would",
"wrap",
"wreck",
"wrestle",
"wriggle",
"x-ray",
"yawn",
"yell",
"zip",
"zoom" };
    }
}

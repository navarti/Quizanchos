namespace Quizanchos.TicketToRideEurope.GameLogic;

public static class TicketToRideEuropeMap
{
    public const string ColorPurple = "PURPLE";
    public const string ColorBlue = "BLUE";
    public const string ColorOrange = "ORANGE";
    public const string ColorWhite = "WHITE";
    public const string ColorGreen = "GREEN";
    public const string ColorYellow = "YELLOW";
    public const string ColorBlack = "BLACK";
    public const string ColorRed = "RED";
    public const string ColorGray = "GRAY";
    public const string ColorLocomotive = "LOCOMOTIVE";

    public static readonly string[] TrainColors =
    {
        ColorPurple, ColorBlue, ColorOrange, ColorWhite,
        ColorGreen, ColorYellow, ColorBlack, ColorRed
    };

    public static readonly string[] PlayerColors =
    {
        "red", "blue", "green", "yellow", "black"
    };

    public static IReadOnlyDictionary<int, int> RouteLengthScores { get; } = new Dictionary<int, int>
    {
        { 1, 1 },
        { 2, 2 },
        { 3, 4 },
        { 4, 7 },
        { 6, 15 },
        { 8, 21 }
    };

    public sealed class CityInfo
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public int X { get; init; }
        public int Y { get; init; }
    }

    public sealed class RouteInfo
    {
        public string Id { get; init; } = "";
        public string CityA { get; init; } = "";
        public string CityB { get; init; } = "";
        public int Length { get; init; }
        public string Color { get; init; } = ColorGray;
        public bool IsTunnel { get; init; }
        public int FerryLocomotives { get; init; }
    }

    public sealed class TicketInfo
    {
        public string Id { get; init; } = "";
        public string CityA { get; init; } = "";
        public string CityB { get; init; } = "";
        public int Points { get; init; }
        public bool IsLong { get; init; }
    }

    public static IReadOnlyList<CityInfo> Cities { get; } = new[]
    {
        new CityInfo { Id = "edinburgh", Name = "Edinburgh", X = 200, Y = 130 },
        new CityInfo { Id = "london", Name = "London", X = 215, Y = 215 },
        new CityInfo { Id = "amsterdam", Name = "Amsterdam", X = 285, Y = 215 },
        new CityInfo { Id = "essen", Name = "Essen", X = 340, Y = 230 },
        new CityInfo { Id = "hamburg", Name = "Hamburg", X = 370, Y = 195 },
        new CityInfo { Id = "kobenhavn", Name = "Kobenhavn", X = 400, Y = 155 },
        new CityInfo { Id = "stockholm", Name = "Stockholm", X = 470, Y = 95 },
        new CityInfo { Id = "petrograd", Name = "Petrograd", X = 620, Y = 85 },
        new CityInfo { Id = "moskva", Name = "Moskva", X = 760, Y = 175 },
        new CityInfo { Id = "smolensk", Name = "Smolensk", X = 685, Y = 215 },
        new CityInfo { Id = "wilno", Name = "Wilno", X = 580, Y = 235 },
        new CityInfo { Id = "riga", Name = "Riga", X = 540, Y = 175 },
        new CityInfo { Id = "danzig", Name = "Danzig", X = 460, Y = 245 },
        new CityInfo { Id = "berlin", Name = "Berlin", X = 405, Y = 260 },
        new CityInfo { Id = "warszawa", Name = "Warszawa", X = 495, Y = 275 },
        new CityInfo { Id = "kyiv", Name = "Kyiv", X = 645, Y = 305 },
        new CityInfo { Id = "kharkov", Name = "Kharkov", X = 745, Y = 305 },
        new CityInfo { Id = "rostov", Name = "Rostov", X = 800, Y = 365 },
        new CityInfo { Id = "sevastopol", Name = "Sevastopol", X = 720, Y = 405 },
        new CityInfo { Id = "bucuresti", Name = "Bucuresti", X = 580, Y = 380 },
        new CityInfo { Id = "budapest", Name = "Budapest", X = 480, Y = 355 },
        new CityInfo { Id = "wien", Name = "Wien", X = 425, Y = 330 },
        new CityInfo { Id = "frankfurt", Name = "Frankfurt", X = 340, Y = 285 },
        new CityInfo { Id = "munchen", Name = "Munchen", X = 385, Y = 320 },
        new CityInfo { Id = "zurich", Name = "Zurich", X = 340, Y = 340 },
        new CityInfo { Id = "venezia", Name = "Venezia", X = 390, Y = 380 },
        new CityInfo { Id = "zagrab", Name = "Zagrab", X = 425, Y = 385 },
        new CityInfo { Id = "sarajevo", Name = "Sarajevo", X = 480, Y = 410 },
        new CityInfo { Id = "sofia", Name = "Sofia", X = 555, Y = 430 },
        new CityInfo { Id = "athina", Name = "Athina", X = 540, Y = 510 },
        new CityInfo { Id = "constantinople", Name = "Constantinople", X = 645, Y = 455 },
        new CityInfo { Id = "smyrna", Name = "Smyrna", X = 645, Y = 510 },
        new CityInfo { Id = "angora", Name = "Angora", X = 720, Y = 470 },
        new CityInfo { Id = "erzurum", Name = "Erzurum", X = 815, Y = 455 },
        new CityInfo { Id = "roma", Name = "Roma", X = 380, Y = 445 },
        new CityInfo { Id = "brindisi", Name = "Brindisi", X = 450, Y = 470 },
        new CityInfo { Id = "palermo", Name = "Palermo", X = 400, Y = 525 },
        new CityInfo { Id = "marseille", Name = "Marseille", X = 280, Y = 380 },
        new CityInfo { Id = "barcelona", Name = "Barcelona", X = 215, Y = 425 },
        new CityInfo { Id = "madrid", Name = "Madrid", X = 130, Y = 445 },
        new CityInfo { Id = "lisboa", Name = "Lisboa", X = 50,  Y = 460 },
        new CityInfo { Id = "cadiz", Name = "Cadiz", X = 70,  Y = 510 },
        new CityInfo { Id = "pamplona", Name = "Pamplona", X = 195, Y = 380 },
        new CityInfo { Id = "brest", Name = "Brest", X = 175, Y = 290 },
        new CityInfo { Id = "paris", Name = "Paris", X = 260, Y = 295 },
        new CityInfo { Id = "dieppe", Name = "Dieppe", X = 240, Y = 250 },
        new CityInfo { Id = "bruxelles", Name = "Bruxelles", X = 290, Y = 255 }
    };

    public static IReadOnlyList<RouteInfo> Routes { get; } = new[]
    {
        // Britain & North Sea
        Route("edinburgh", "london", 4, ColorBlack),
        Route("london", "dieppe", 2, ColorGray, ferryLoco: 1),
        Route("london", "amsterdam", 2, ColorGray, ferryLoco: 2),
        Route("dieppe", "brest", 2, ColorOrange),
        Route("dieppe", "paris", 1, ColorPurple),
        Route("dieppe", "bruxelles", 2, ColorGreen),
        Route("brest", "paris", 3, ColorBlack),
        Route("brest", "pamplona", 4, ColorPurple),

        // France
        Route("paris", "bruxelles", 2, ColorYellow),
        Route("paris", "frankfurt", 3, ColorWhite),
        Route("paris", "zurich", 3, ColorGray, isTunnel: true),
        Route("paris", "marseille", 4, ColorGray),
        Route("paris", "pamplona", 4, ColorBlue),
        Route("marseille", "zurich", 2, ColorPurple),
        Route("marseille", "roma", 4, ColorGray, isTunnel: true),
        Route("marseille", "barcelona", 4, ColorGray),

        // Iberia
        Route("barcelona", "pamplona", 2, ColorGray),
        Route("barcelona", "madrid", 2, ColorYellow),
        Route("madrid", "pamplona", 3, ColorBlack),
        Route("madrid", "lisboa", 3, ColorPurple),
        Route("madrid", "cadiz", 3, ColorOrange),
        Route("lisboa", "cadiz", 2, ColorBlue),

        // Low Countries & Germany
        Route("bruxelles", "amsterdam", 1, ColorBlack),
        Route("bruxelles", "frankfurt", 2, ColorBlue),
        Route("amsterdam", "essen", 3, ColorYellow),
        Route("amsterdam", "frankfurt", 2, ColorWhite),
        Route("essen", "frankfurt", 2, ColorGreen),
        Route("essen", "berlin", 2, ColorBlue),
        Route("essen", "kobenhavn", 3, ColorGray, ferryLoco: 1),
        Route("frankfurt", "berlin", 3, ColorBlack),
        Route("frankfurt", "munchen", 2, ColorPurple),
        Route("munchen", "berlin", 2, ColorGreen),
        Route("munchen", "zurich", 2, ColorYellow, isTunnel: true),
        Route("munchen", "venezia", 2, ColorBlue, isTunnel: true),
        Route("munchen", "wien", 3, ColorOrange),
        Route("zurich", "venezia", 2, ColorGreen, isTunnel: true),

        // Germany East / Baltic
        Route("berlin", "hamburg", 1, ColorBlue),
        Route("berlin", "danzig", 4, ColorGray),
        Route("berlin", "warszawa", 4, ColorYellow),
        Route("hamburg", "essen", 1, ColorRed),
        Route("hamburg", "kobenhavn", 2, ColorGray, ferryLoco: 1),
        Route("hamburg", "danzig", 3, ColorGray),
        Route("kobenhavn", "stockholm", 3, ColorYellow, ferryLoco: 1),
        Route("stockholm", "petrograd", 8, ColorGray),
        Route("danzig", "warszawa", 2, ColorGray),
        Route("danzig", "riga", 3, ColorBlack),

        // Russia & Baltic East
        Route("riga", "petrograd", 4, ColorGray),
        Route("riga", "wilno", 4, ColorGreen),
        Route("petrograd", "wilno", 4, ColorBlue),
        Route("petrograd", "moskva", 4, ColorWhite),
        Route("wilno", "warszawa", 3, ColorRed),
        Route("wilno", "smolensk", 3, ColorYellow),
        Route("wilno", "kyiv", 2, ColorGray),
        Route("smolensk", "moskva", 2, ColorOrange),
        Route("smolensk", "kyiv", 3, ColorRed),
        Route("moskva", "kharkov", 4, ColorGray),
        Route("kharkov", "kyiv", 4, ColorGray),
        Route("kharkov", "rostov", 2, ColorGreen),
        Route("rostov", "sevastopol", 4, ColorGray),
        Route("rostov", "kharkov", 2, ColorGreen),

        // Central Europe / Balkans
        Route("warszawa", "wien", 4, ColorBlue),
        Route("warszawa", "kyiv", 4, ColorGray),
        Route("wien", "budapest", 1, ColorWhite),
        Route("wien", "zagrab", 2, ColorGray),
        Route("budapest", "kyiv", 6, ColorGray, isTunnel: true),
        Route("budapest", "bucuresti", 4, ColorGray),
        Route("budapest", "sarajevo", 3, ColorPurple),
        Route("budapest", "zagrab", 2, ColorOrange),
        Route("kyiv", "bucuresti", 4, ColorGray, isTunnel: true),
        Route("bucuresti", "sofia", 2, ColorGray),
        Route("bucuresti", "constantinople", 3, ColorYellow),
        Route("bucuresti", "sevastopol", 4, ColorWhite, ferryLoco: 1),
        Route("zagrab", "sarajevo", 3, ColorRed),
        Route("zagrab", "venezia", 2, ColorGray, isTunnel: true),
        Route("sarajevo", "sofia", 2, ColorGray, isTunnel: true),
        Route("sarajevo", "athina", 4, ColorGreen),
        Route("sofia", "athina", 3, ColorPurple),
        Route("sofia", "constantinople", 3, ColorBlue),

        // Italy & Mediterranean
        Route("venezia", "roma", 2, ColorBlack, isTunnel: true),
        Route("roma", "brindisi", 2, ColorWhite),
        Route("roma", "palermo", 4, ColorGray, ferryLoco: 1),
        Route("brindisi", "palermo", 3, ColorGray, ferryLoco: 1),
        Route("brindisi", "athina", 4, ColorGray, ferryLoco: 1),

        // Eastern Med / Asia Minor
        Route("athina", "smyrna", 2, ColorGray, ferryLoco: 1),
        Route("smyrna", "constantinople", 2, ColorGray, isTunnel: true),
        Route("smyrna", "angora", 3, ColorOrange),
        Route("smyrna", "palermo", 6, ColorGray, ferryLoco: 2),
        Route("constantinople", "angora", 2, ColorGray),
        Route("angora", "erzurum", 3, ColorBlack),
        Route("erzurum", "sevastopol", 4, ColorGray, ferryLoco: 2),
        Route("constantinople", "sevastopol", 4, ColorGray, ferryLoco: 2)
    };

    public static IReadOnlyList<TicketInfo> Tickets { get; } = new[]
    {
        // Long destination tickets (blue background)
        Ticket("brest", "petrograd", 20, isLong: true),
        Ticket("cadiz", "stockholm", 21, isLong: true),
        Ticket("edinburgh", "athina", 21, isLong: true),
        Ticket("lisboa", "danzig", 20, isLong: true),
        Ticket("palermo", "moskva", 20, isLong: true),
        Ticket("kobenhavn", "erzurum", 21, isLong: true),

        // Regular destination tickets
        Ticket("amsterdam", "pamplona", 7),
        Ticket("amsterdam", "wilno", 12),
        Ticket("angora", "kharkov", 10),
        Ticket("athina", "wilno", 11),
        Ticket("barcelona", "bruxelles", 8),
        Ticket("barcelona", "munchen", 8),
        Ticket("berlin", "bucuresti", 8),
        Ticket("berlin", "moskva", 12),
        Ticket("berlin", "roma", 9),
        Ticket("brest", "marseille", 7),
        Ticket("brest", "venezia", 8),
        Ticket("bruxelles", "danzig", 9),
        Ticket("budapest", "sofia", 5),
        Ticket("cadiz", "kobenhavn", 17),
        Ticket("edinburgh", "paris", 7),
        Ticket("essen", "kyiv", 10),
        Ticket("frankfurt", "kobenhavn", 5),
        Ticket("frankfurt", "smolensk", 13),
        Ticket("kyiv", "petrograd", 6),
        Ticket("kyiv", "rostov", 9),
        Ticket("london", "berlin", 7),
        Ticket("london", "wien", 10),
        Ticket("madrid", "dieppe", 8),
        Ticket("madrid", "zurich", 8),
        Ticket("marseille", "essen", 8),
        Ticket("palermo", "constantinople", 8),
        Ticket("paris", "wien", 8),
        Ticket("paris", "zagrab", 7),
        Ticket("riga", "bucuresti", 10),
        Ticket("roma", "smyrna", 8),
        Ticket("rostov", "erzurum", 5),
        Ticket("sarajevo", "sevastopol", 8),
        Ticket("sofia", "smyrna", 5),
        Ticket("stockholm", "wien", 11),
        Ticket("venezia", "constantinople", 10),
        Ticket("warszawa", "smolensk", 6),
        Ticket("zagrab", "brindisi", 6),
        Ticket("zurich", "brindisi", 6),
        Ticket("zurich", "budapest", 6)
    };

    private static readonly HashSet<string> CityIdSet = new(Cities.Select(c => c.Id));

    public static IReadOnlyList<TicketInfo> ValidatedTickets { get; } =
        Tickets.Where(t => CityIdSet.Contains(t.CityA) && CityIdSet.Contains(t.CityB)).ToList();

    public static List<string> BuildTrainCardDeck()
    {
        // 12 of each color + 14 locomotives = 110 cards
        List<string> deck = new(110);
        foreach (string color in TrainColors)
        {
            for (int i = 0; i < 12; i++)
            {
                deck.Add(color);
            }
        }
        for (int i = 0; i < 14; i++)
        {
            deck.Add(ColorLocomotive);
        }
        return deck;
    }

    public static IReadOnlyDictionary<string, CityInfo> CityById { get; } =
        Cities.ToDictionary(c => c.Id);

    public static IReadOnlyDictionary<string, RouteInfo> RouteById { get; } =
        Routes.ToDictionary(r => r.Id);

    public static IReadOnlyDictionary<string, TicketInfo> TicketById { get; } =
        ValidatedTickets.ToDictionary(t => t.Id);

    public static IReadOnlyDictionary<string, List<RouteInfo>> RoutesByCity { get; } = BuildRoutesByCity();

    private static Dictionary<string, List<RouteInfo>> BuildRoutesByCity()
    {
        Dictionary<string, List<RouteInfo>> map = new();
        foreach (RouteInfo route in Routes)
        {
            if (!map.TryGetValue(route.CityA, out List<RouteInfo>? listA))
            {
                listA = new List<RouteInfo>();
                map[route.CityA] = listA;
            }
            listA.Add(route);

            if (!map.TryGetValue(route.CityB, out List<RouteInfo>? listB))
            {
                listB = new List<RouteInfo>();
                map[route.CityB] = listB;
            }
            listB.Add(route);
        }
        return map;
    }

    private static RouteInfo Route(string a, string b, int length, string color,
        bool isTunnel = false, int ferryLoco = 0)
    {
        return new RouteInfo
        {
            Id = $"{a}__{b}",
            CityA = a,
            CityB = b,
            Length = length,
            Color = color,
            IsTunnel = isTunnel,
            FerryLocomotives = ferryLoco
        };
    }

    private static TicketInfo Ticket(string a, string b, int points, bool isLong = false)
    {
        return new TicketInfo
        {
            Id = $"{a}__{b}",
            CityA = a,
            CityB = b,
            Points = points,
            IsLong = isLong
        };
    }
}

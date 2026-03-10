using System;
using System.Collections.Generic;

namespace Data.Adventures
{
    [Serializable]
    public class Scenario
    {
        public long id;
        public string state;
    }

    [Serializable]
    public class Stage
    {
        public long id;
        public string state;
        public List<Scenario> scenarios;
    }

    [Serializable]
    public class Adventure
    {
        public long id;
        public string state;
        public List<Stage> stages;
    }

    [Serializable]
    public class AdventuresResponse
    {
        public List<Adventure> adventures;
    }
}
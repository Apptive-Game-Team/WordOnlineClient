using System;
using System.Collections.Generic;

namespace Data.Adventures.Dto
{
    [Serializable]
    public class ScenarioDto
    {
        public long id;
        public string state;
    }

    [Serializable]
    public class StageDto
    {
        public long id;
        public string state;
        public List<ScenarioDto> scenarios;
    }

    [Serializable]
    public class AdventureDto
    {
        public long id;
        public string state;
        public List<StageDto> stages;
    }

    [Serializable]
    public class AdventuresResponseDto
    {
        public List<AdventureDto> adventures;
    }
}
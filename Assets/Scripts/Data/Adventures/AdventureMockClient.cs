using System;
using System.Collections;
using System.Collections.Generic;
using Data.Adventures.Dto;

namespace Data.Adventures
{
    public class AdventureMockClient : AdventureClient
    {
        public override IEnumerator GetAdventure(Action<AdventuresResponseDto> callback)
        {
            callback.Invoke(
                new AdventuresResponseDto()
                {
                    adventures = new List<AdventureDto>()
                    {
                        new AdventureDto()
                        {
                            id = 1,
                            state = "ACTIVE",
                            stages = new List<StageDto>()
                            {
                                new StageDto()
                                {
                                    id = 1,
                                    state = "ACTIVE",
                                    scenarios = new List<ScenarioDto>()
                                    {
                                        new ScenarioDto() { id = 1, state = "FINISHED" },
                                        new ScenarioDto() { id = 2, state = "FINISHED" },
                                        new ScenarioDto() { id = 3, state = "ACTIVE" },
                                        new ScenarioDto() { id = 4, state = "INACTIVE" },
                                    }
                                },
                                new StageDto()
                                {
                                    id = 2,
                                    state = "INACTIVE",
                                    scenarios = new List<ScenarioDto>()
                                    {
                                    }
                                },
                            }
                        },
                        new AdventureDto()
                        {
                            id = 2,
                            state = "INACTIVE",
                            stages = new List<StageDto>()
                            {
                            }
                        }
                    }

                }
            );
            yield break;
        }
    }
}
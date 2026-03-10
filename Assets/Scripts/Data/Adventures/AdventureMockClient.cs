using System;
using System.Collections;
using System.Collections.Generic;

namespace Data.Adventures
{
    public class AdventureMockClient : AdventureClient
    {
        public override IEnumerator GetAdventure(Action<AdventuresResponse> callback)
        {
            callback.Invoke(
                new AdventuresResponse()
                {
                    adventures = new List<Adventure>()
                    {
                        new Adventure()
                        {
                            id = 1,
                            state = "ACTIVE",
                            stages = new List<Stage>()
                            {
                                new Stage()
                                {
                                    id = 1,
                                    scenarios = new List<Scenario>()
                                    {
                                        new Scenario()
                                        {
                                            id = 1, state = "ACTIVE"
                                        },
                                        new Scenario()
                                        {
                                            id = 2, state = "INACTIVE"
                                        },
                                    }
                                },
                                new Stage()
                                {
                                    id = 2,
                                    scenarios = new List<Scenario>()
                                    {
                                        new Scenario()
                                        {
                                            id = 3, state = "ACTIVE"
                                        },
                                        new Scenario()
                                        {
                                            id = 4, state = "INACTIVE"
                                        },
                                    }
                                },
                            }
                        }
                    }

                }
            );
            yield break;
        }
    }
}
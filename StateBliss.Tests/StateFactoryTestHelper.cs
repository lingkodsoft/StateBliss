using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SimpleInjector;

//https://www.codingame.com/playgrounds/35462/creating-web-api-in-asp-net-core-2-0/part-3---integration-tests
namespace StateBliss.Tests
{
    public static class StateFactoryTestHelper
    {
        private static readonly ConcurrentDictionary<Type, List<State>> StatesRepo = new ConcurrentDictionary<Type, List<State>>();
        public static StateFactory SetupStateFactory(Guid guid, Func<State> stateProvider)
        {
            StateFactory result = (stateType, id) =>
            {
                State state0;

                if (!StatesRepo.ContainsKey(stateType))
                {
                    state0 = stateProvider();
                    StatesRepo.TryAdd(stateType, new List<State>() { state0 });
                }
                else
                {
                    var states = StatesRepo[stateType];
                    state0 = states.FirstOrDefault(a => a.Id == guid);

                    if (state0 == null)
                    {
                        state0 = stateProvider();
                        states.Add(state0);
                    }
                }
                return state0;
            };

            return result;
        }
    }
}
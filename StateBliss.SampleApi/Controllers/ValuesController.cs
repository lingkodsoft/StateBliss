using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Remotion.Linq.Clauses;

namespace StateBliss.SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IStateMachineManager _stateMachineManager;
        private readonly OrdersRepository _ordersRepository;

        public ValuesController(IStateMachineManager stateMachineManager, OrdersRepository ordersRepository)
        {
            _stateMachineManager = stateMachineManager;
            _ordersRepository = ordersRepository;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var uid = Guid.NewGuid();
            var cmd = new PayOrderTriggerContext
            {
                Order = new Order
                {
                    Id = 1,
                    Uid = uid,
                    State = OrderState.Initial
                },
                Uid = uid,
                ToState = OrderState.Paid
            };
            
            _ordersRepository.InsertOrder(cmd.Order);

            _stateMachineManager.Trigger(cmd);
            
            
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

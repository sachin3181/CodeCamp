using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [ApiController, Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repo;
        private readonly IMapper _mapper;
        private LinkGenerator _linkGenerator;

        public TalksController(ICampRepository repo, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repo = repo;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await _repo.GetTalksByMonikerAsync(moniker, true);
                if (talks == null)
                {
                    return NotFound($"Talks not found for moniker {moniker}");
                }

                return _mapper.Map<TalkModel[]>(talks);
            }
            catch ( Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to get talks");
            }
        }

        [HttpGet("{talkId:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int talkId)
        {
            try
            {
                var talk = await _repo.GetTalkByMonikerAsync(moniker, talkId, true);
                if (talk == null)
                {
                    return NotFound($"Talk not found for moniker {moniker}");
                }

                return _mapper.Map<TalkModel>(talk);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get talk for moniker {moniker}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await _repo.GetCampAsync(moniker);
                if (camp == null)
                {
                    return NotFound($"camp not found for moniker {moniker}");
                }

                var talk = _mapper.Map<Talk>(model);
                talk.Camp = camp;
                if (model.Speaker == null)
                {
                    return BadRequest("speaker id is required");
                }
         
                var speaker = await _repo.GetSpeakerAsync(model.Speaker.SpeakerId);

                if (speaker == null)
                {
                    return BadRequest("speaker could not be found");
                }
                talk.Speaker = speaker;

                _repo.Add(talk);
                if (await _repo.SaveChangesAsync())
                {
                    var url = _linkGenerator.GetPathByAction(HttpContext,
                        "Get",
                        values: new { moniker, talkId = talk.TalkId });

                    return Created(url, _mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest("Failed to save new talk");
                }

               
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get talk for moniker {moniker}");
            }
        }

        [HttpPut("{talkId:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int talkId, TalkModel model)
        {
            try
            {
                var talk = await _repo.GetTalkByMonikerAsync(moniker, talkId, true);
                if (talk == null)
                {
                    return NotFound($"Talk not found for moniker {moniker}");
                }
                if (model.Speaker != null)
                {
                    var speaker = await _repo.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }
                _mapper.Map(model, talk);

                if (await _repo.SaveChangesAsync())
                {
                    return _mapper.Map<TalkModel>(talk);
                }
                
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get talk for moniker {moniker}");
            }

            return BadRequest();
        }

        [HttpDelete("{talkId:int}")]
        public async Task<IActionResult> Delete(string moniker, int talkId)
        {
            try
            {
                var talk = await _repo.GetTalkByMonikerAsync(moniker, talkId);
                if (talk == null)
                {
                    return NotFound($"Talk not found for moniker {moniker}");
                }
                _repo.Delete(talk);

                if (await _repo.SaveChangesAsync())
                {
                    return Ok();
                }
               
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Failed to get talk for moniker {moniker}");
            }

            return BadRequest();
        }
    }
}
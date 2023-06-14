using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StatisGoat.Accounts;
using StatisGoat.Authentication;
using StatisGoat.Chats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace StatisGoat.Api.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatRepository chatRepository;
        private readonly IAuthenticationRepository authRepository;
        private readonly IConfiguration configuration;

        public ChatController(IChatRepository cr, IConfiguration config, IAuthenticationRepository ar)
        {
            chatRepository = cr;
            authRepository = ar;
            configuration = config;
        }

        [HttpGet]
        [Route("chats/backfill")]
        public override async Task<IActionResult> Backfill()
        {
            return Ok();
        }

        [HttpGet]
        [Route("chats/match")]
        public async Task<IActionResult> GetByMID(int mid, string? session)
        {
            if (session != null)
            {
                AccountRecord account = await authRepository.GetAccountBySessionAsync(session);
                if (account is null) { return BadRequest(); }
                else { return Ok(await chatRepository.FindByMID(mid, account.Id)); }
            }
            else
            {
                return Ok(await chatRepository.FindByMID(mid, null));
            }
        }

        [HttpPost]
        [Route("chats/post")]
        public async Task<IActionResult> Post(int mid, string session, string? message, DateTime? timestamp, int? threadid)
        {
            AccountRecord account = await authRepository.GetAccountBySessionAsync(session);
            if (account is null) { return BadRequest(); }
            else
            {
                await chatRepository.SaveAsync(new ChatRecord
                {
                    MatchID = mid,
                    AccountID = account.Id,
                    Message = message,
                    Timestamp = timestamp,
                    ThreadID = threadid
                });
                return Ok();
            }
        }

        [HttpDelete]
        [Route("chats/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            await chatRepository.DeleteAsync(id);
            return Ok();
        }

        [HttpPost]
        [Route("chats/like")]
        public async Task<IActionResult> Like(string session, int chatid)
        {
            AccountRecord account = await authRepository.GetAccountBySessionAsync(session);
            if (account is null) { return BadRequest(); }
            else
            {
                await chatRepository.LikeAsync(account.Id, chatid);
                return Ok();
            }
        }

        [HttpPost]
        [Route("chats/unlike")]
        public async Task<IActionResult> Unlike(string session, int chatid)
        {
            AccountRecord account = await authRepository.GetAccountBySessionAsync(session);
            if (account is null) { return BadRequest(); }
            else
            {
                await chatRepository.UnlikeAsync(account.Id, chatid);
                return Ok();
            }
        }

        [HttpPost]
        [Route("chats/dislike")]
        public async Task<IActionResult> Dislike(string session, int chatid)
        {
            AccountRecord account = await authRepository.GetAccountBySessionAsync(session);
            if (account is null) { return BadRequest(); }
            else
            {
                await chatRepository.DislikeAsync(account.Id, chatid);
                return Ok();
            }
        }

        [HttpPost]
        [Route("chats/undislike")]
        public async Task<IActionResult> Undislike(string session, int chatid)
        {
            AccountRecord account = await authRepository.GetAccountBySessionAsync(session);
            if (account is null) { return BadRequest(); }
            else
            {
                await chatRepository.UndislikeAsync(account.Id, chatid);
                return Ok();
            }
        }
    }
}

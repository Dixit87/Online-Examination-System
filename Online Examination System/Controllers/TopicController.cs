using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Online_Examination_System.Models;
using Online_Examination_System.Repositories;
using Online_Examination_System.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Online_Examination_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TopicController : Controller
    {
        private readonly ITopicRepository _topicRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly ISessionRepository _sessionRepository;

        public TopicController(
            ITopicRepository topicRepository,
            IChapterRepository chapterRepository,
            ISessionRepository sessionRepository)
        {
            _topicRepository = topicRepository;
            _chapterRepository = chapterRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var topics = await _topicRepository.GetAllAsync();
            return View(topics);
        }

        [HttpGet]
        public async Task<IActionResult> GetChaptersBySession(int sessionId)
        {
            var chapters = await _chapterRepository.GetAllAsync();
            var filtered = chapters.Where(c => c.SessionId == sessionId && c.Status).Select(c => new
            {
                value = c.Id,
                text = c.Title
            }).ToList();
            return Json(filtered);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var model = new TopicUpsertViewModel();
            
            // Populate Session Dropdown
            var sessions = await _sessionRepository.GetAllAsync();
            model.SessionList = sessions.Where(s => s.Status).Select(s => new SelectListItem
            {
                Text = s.Title,
                Value = s.Id.ToString()
            });

            // Empty chapter list initially unless editing
            model.ChapterList = new List<SelectListItem>();

            if (id.HasValue && id > 0)
            {
                var topic = await _topicRepository.GetByIdAsync(id.Value);
                if (topic == null) return NotFound();
                
                model.Id = topic.Id;
                model.SessionId = topic.SessionId;
                model.ChapterId = topic.ChapterId;
                model.Title = topic.Title;
                model.Description = topic.Description;
                model.DurationMin = topic.DurationMin;
                model.TopicType = topic.TopicType;
                model.TopicFileType = topic.TopicFileType;
                model.TopicFilePath = topic.TopicFilePath;
                model.TopicPosition = topic.TopicPosition;
                model.Status = topic.Status;

                // Populate Chapter Dropdown for this session
                var chapters = await _chapterRepository.GetAllAsync();
                model.ChapterList = chapters.Where(c => c.SessionId == topic.SessionId && c.Status).Select(c => new SelectListItem
                {
                    Text = c.Title,
                    Value = c.Id.ToString()
                });

                // Load Details
                var details = await _topicRepository.GetTopicDetailsByTopicIdAsync(topic.Id);
                model.TopicDetails = details.Select(d => new TopicDetailViewModel
                {
                    Id = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    DurationMin = d.DurationMin,
                    FileType = d.FileType,
                    FilePath = d.FilePath,
                    Position = d.Position,
                    Status = d.Status
                }).ToList();
            }
            else
            {
                model.TopicType = "General"; // Default
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(TopicUpsertViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                var topic = new Topic
                {
                    Id = model.Id,
                    SessionId = model.SessionId,
                    ChapterId = model.ChapterId,
                    Title = model.Title,
                    Description = model.Description,
                    DurationMin = model.DurationMin,
                    TopicType = model.TopicType,
                    TopicFileType = model.TopicFileType,
                    TopicFilePath = model.TopicFilePath,
                    TopicPosition = model.TopicPosition,
                    Status = model.Status,
                    CreatedBy = currentUserId
                };

                // Upsert Topic and get new ID
                int savedTopicId = await _topicRepository.UpsertTopicAsync(topic);
                
                // Process Topic Details
                if (model.TopicType == "Accordion" || model.TopicType == "Multi Slider")
                {
                    var detailsList = new List<TopicDetail>();
                    if (model.TopicDetails != null)
                    {
                        foreach (var d in model.TopicDetails.Where(x => !x.IsDeleted))
                        {
                            detailsList.Add(new TopicDetail
                            {
                                SessionId = model.SessionId,
                                ChapterId = model.ChapterId,
                                TopicId = savedTopicId,
                                Title = d.Title ?? "Untitled",
                                Description = d.Description,
                                DurationMin = d.DurationMin,
                                FileType = d.FileType,
                                FilePath = d.FilePath,
                                Position = d.Position,
                                Status = d.Status,
                                CreatedBy = currentUserId
                            });
                        }
                    }
                    await _topicRepository.UpsertTopicDetailsAsync(savedTopicId, detailsList);
                }
                else
                {
                    // If type is General, clear out details just in case it was changed from Accordion
                    await _topicRepository.UpsertTopicDetailsAsync(savedTopicId, new List<TopicDetail>());
                }

                TempData["SuccessMessage"] = "Topic saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            // Repopulate Dropdowns on error
            var sessions = await _sessionRepository.GetAllAsync();
            model.SessionList = sessions.Where(s => s.Status).Select(s => new SelectListItem
            {
                Text = s.Title,
                Value = s.Id.ToString()
            });
            
            if (model.SessionId > 0)
            {
                var chapters = await _chapterRepository.GetAllAsync();
                model.ChapterList = chapters.Where(c => c.SessionId == model.SessionId && c.Status).Select(c => new SelectListItem
                {
                    Text = c.Title,
                    Value = c.Id.ToString()
                });
            }
            else
            {
                model.ChapterList = new List<SelectListItem>();
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            await _topicRepository.ToggleStatusAsync(id);
            TempData["SuccessMessage"] = "Topic status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Common;
using DoddleReport;
using DoddleReport.Web;
using IServices.Infrastructure;
using IServices.ISysServices;
using Models.SysModels;

namespace Web.Areas.Platform.Controllers
{
    public class SysUserController : Controller
    {
        private readonly ISysDepartmentService _sysDepartmentService;
        private readonly ISysDepartmentSysUserService _sysDepartmentSysUserService;
        private readonly ISysRoleService _sysRoleService;
        private readonly ISysRoleSysUserService _sysRoleSysUserService;
        private readonly ISysUserService _sysUserService;
        private readonly IUnitOfWork _unitOfWork;

        public SysUserController(IUnitOfWork unitOfWork, ISysDepartmentService sysDepartmentService,
            ISysDepartmentSysUserService sysDepartmentSysUserService,
            ISysUserService sysUserService, ISysRoleService sysRoleService,
            ISysRoleSysUserService sysRoleSysUserService)
        {
            _unitOfWork = unitOfWork;
            _sysDepartmentService = sysDepartmentService;
            _sysDepartmentSysUserService = sysDepartmentSysUserService;
            _sysUserService = sysUserService;
            _sysRoleService = sysRoleService;
            _sysRoleSysUserService = sysRoleSysUserService;
        }

        //
        // GET: /Platform/SysUser/

        public ActionResult Index(string keyword, string ordering, int pageIndex = 1)
        {
            var model =
                _sysUserService.GetAll()
                    .Select(
                        a =>
                            new
                            {
                                a.UserName,
                                a.DisplayName,
                                a.Email,
                                a.Enabled,
                                a.CreatedDate,
                                RoleName = a.SysRoleSysUsers.Select(b => b.SysRole.RoleName + "  "),
                                a.SysUserLogs.Count,
                                a.Remark,
                                a.Id
                            }).Search(keyword);


            if (!string.IsNullOrEmpty(ordering))
            {
                model = model.OrderBy(ordering, null);
            }

            if (!string.IsNullOrEmpty(Request["report"]))
            {
                //导出
                return new ReportResult(new Report(model.ToReportSource()));
            }

            return View(model.ToPagedList(pageIndex));
        }

        //
        // GET: /Platform/SysUser/Details/5

        public ActionResult Details(Guid id)
        {
            SysUser item = _sysUserService.GetById(id);

            ViewBag.SysDepartmentsId = String.Join(" ",
                item.SysDepartmentSysUsers.Select(a => a.SysDepartment.DepartmentName));

            ViewBag.SysRolesId = String.Join(" ", item.SysRoleSysUsers.Select(a => a.SysRole.RoleName));

            return View(item);
        }

        public ActionResult Create()
        {
            return RedirectToAction("Edit");
        }

        //
        // GET: /Platform/SysUser/Edit/5
        /// <summary>
        /// Creat or Edit Existing UserAccount
        /// </summary>
        /// <param name="id">UserId</param>
        public ActionResult Edit(Guid? id)
        {
            //User Account Model
            SysUser item = new SysUser();

            //If Id Has value then select 
            if (id.HasValue)
                item = _sysUserService.GetById(id.Value);

            ViewBag.SysDepartmentsId = new MultiSelectList(_sysDepartmentService.GetAll(), "Id", "DepartmentName",
                item.SysDepartmentSysUsers.Select(a => a.SysDepartmentId));

            ViewBag.SysRolesId = new MultiSelectList(_sysRoleService.GetAll(), "Id", "RoleName",
                item.SysRoleSysUsers.Select(a => a.SysRoleId));

            return View(item);
        }

        //
        // POST: /Platform/SysUser/Edit/5

        [HttpPost]
        public ActionResult Edit(Guid? id, SysUser _sysUser)
        {
            Debug.WriteLine("a");

            if (!ModelState.IsValid)
            {
                Edit(id);
                return View(_sysUser);
            }

            if (id.HasValue)
            {
                //清除原有部门数据
                _sysDepartmentSysUserService.Delete(a => a.SysUserId.Equals(id.Value));
                //清除原有数据
                _sysRoleSysUserService.Delete(a => a.SysUserId.Equals(id.Value));
            }

            _sysUserService.Save(id, _sysUser);

            if (_sysUser.SysDepartmentsId != null)
            {
                foreach (Guid sysDepartmentId in _sysUser.SysDepartmentsId)
                {
                    _sysDepartmentSysUserService.Save(null, new SysDepartmentSysUser
                    {
                        SysUserId = _sysUser.Id,
                        SysDepartmentId = sysDepartmentId
                    });
                }
            }

            if (_sysUser.SysRolesId != null)
            {
                foreach (Guid sysRoleId in _sysUser.SysRolesId)
                {
                    _sysRoleSysUserService.Save(null, new SysRoleSysUser
                    {
                        SysUserId = _sysUser.Id,
                        SysRoleId = sysRoleId
                    });
                }
            }

            _unitOfWork.Commit();

            return RedirectToAction("Index");
        }

        //
        // POST: /Platform/SysUser/Delete/5

        [HttpDelete]
        public ActionResult Delete(Guid id)
        {
            _sysUserService.Delete(id);
            _unitOfWork.Commit();
            return RedirectToAction("Index");
        }
    }
}
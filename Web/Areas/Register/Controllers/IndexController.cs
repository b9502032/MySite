using BootstrapSupport;
using Common;
using Models.SysModels;
using Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;


namespace Web.Areas.Register.Controllers
{
    /// <summary>
    /// Enterprise Regist Page
    /// </summary>
    public class IndexController : Controller
    {
        private readonly ApplicationDb applicationDbContext = new ApplicationDb();


        public ActionResult UserIndex()
        {
            return View();
        }

        // 企业注册模块
        // GET: /Initialize/Index/

        public ActionResult Index()
        {
            ////判断系统内企业是否有有效用户
            //if (_db.SysEnterprises.Any(a => !a.Deleted))
            //{
            //    return null;
            //}

            //if (_db.SysUsers.Any(a => !a.Deleted))
            //{
            //    return null;
            //}

            ////显示系统初始化界面 企业及管理员 


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(InitializeModel item)
        {
            if (applicationDbContext.SysEnterprises.Any(u => !u.Deleted && u.EnterpriseName == item.EnterpriseName))
            {
                ModelState.AddModelError("EnterpriseName", @"该企业名称已存在");
            }

            if (applicationDbContext.SysEnterprises.Any(u => !u.Deleted && u.EnterpriseShortName == item.EnterpriseShortName))
            {
                ModelState.AddModelError("EnterpriseShortName", @"该企业简称已存在");
            }

            if (!ModelState.IsValid)
            {
                return View(item);
            }

            try
            {
                #region SysEnterprise

                var host = Spell.MakeSpellCode(item.EnterpriseShortName, SpellOptions.FirstLetterOnly);

                //检查Host是否已经被占用

                while (applicationDbContext.SysEnterprises.Any(a => a.Host == host))
                {
                    host = host + new Random().Next(11, 99);
                }

                host = host + ".wjw1.com";

                var sysEnterprise = new SysEnterprise { EnterpriseName = item.EnterpriseName, EnterpriseShortName = item.EnterpriseShortName, Host = host };

                applicationDbContext.SysEnterprises.Add(sysEnterprise);

                #endregion

                //添加管理员权限栏位
                #region SysRole

                //var sysRole = new SysRole { RoleName = "管理员", EnterpriseId = sysEnterprise.Id };
                var sysRole = new SysRole() { RoleName = "管理员" };

                applicationDbContext.SysRoles.Add(sysRole);

                #endregion

                //添加管理员账号
                #region Add Administrator User

                var sysUser = new SysUser
                {
                    //EnterpriseId = sysEnterprise.Id,
                    UserName = item.UserName,
                    DisplayName = "系统管理员",
                    Email = item.Email,
                    Password = HashPassword.GetHashPassword(item.Password),
                    OldPassword = HashPassword.GetHashPassword(item.Password),
                };

                applicationDbContext.SysUsers.Add(sysUser);

                #endregion

                //对应管理员账号和管理员权限
                #region SysRoleSysUser

                var sysRoleSysUser = new SysRoleSysUser
                {
                    //EnterpriseId = sysEnterprise.Id,
                    SysUserId = sysUser.Id,
                    SysRoleId = sysRole.Id
                };

                applicationDbContext.SysRoleSysUsers.Add(sysRoleSysUser);

                #endregion

                //添加详细权限给予管理员角色
                #region SysRoleSysControllerSysAction

                var sysRoleSysControllerSysAction = new List<SysRoleSysControllerSysAction>();

                foreach (var sysControllerSysAction in applicationDbContext.SysControllerSysActions)
                {
                    sysRoleSysControllerSysAction.Add(new SysRoleSysControllerSysAction
                    {
                        //EnterpriseId = sysEnterprise.Id,
                        SysControllerSysActionId = sysControllerSysAction.Id,
                        SysRoleId = sysRole.Id
                    });
                }

                sysRoleSysControllerSysAction.ForEach(a => applicationDbContext.SysRoleSysControllerSysActions.Add(a));

                #endregion

                applicationDbContext.SaveChanges();

                TempData[Alerts.SUCCESS] = "注册成功！";

                return RedirectToAction("Index", "Succes", new { Id = sysEnterprise.Id });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.Message);
                return View(item);
            }
        }

        /// <summary>
        /// 企業註冊用ViewModel
        /// </summary>
        public class InitializeModel
        {
            [Required(ErrorMessage = @"企业名称不能为空")]
            [StringLength(50, MinimumLength = 4, ErrorMessage = @"{0}长度{2}~{1}")]
            [Remote("CheckUserAccountExists", "Index", ErrorMessage = @"该企业名称已存在")] // 远程验证（Ajax）
            public string EnterpriseName { get; set; }


            [Required(ErrorMessage = @"企业简称不能为空")]
            [StringLength(50, MinimumLength = 4, ErrorMessage = @"{0}长度{2}~{1}")]
            [Remote("CheckEnterpriseShortNameExists", "Index", ErrorMessage = @"该企业简称已存在")] // 远程验证（Ajax）
            public string EnterpriseShortName { get; set; }


            [Required(ErrorMessage = @"用户名不能为空")]
            [StringLength(50, MinimumLength = 4, ErrorMessage = @"{0}长度{2}~{1}")]
            public string UserName { get; set; }

            [DataType(DataType.Password)]
            [Required(ErrorMessage = @"密码不能为空")]
            [StringLength(50, MinimumLength = 5, ErrorMessage = @"{0}长度{2}~{1}")]
            public string Password { get; set; }


            [DataType(DataType.Password)]
            [Required(ErrorMessage = @"密码不能为空")]
            [System.ComponentModel.DataAnnotations.Compare("Password")]
            public string ConfirmPassword { get; set; }


            [EmailAddress(ErrorMessage = @"请输入正确的邮箱")]
            public string Email { get; set; }
        }

        [HttpGet] // 只能用GET ！！！
        public ActionResult CheckUserAccountExists(string enterpriseName)
        {
            var exists = applicationDbContext.SysEnterprises.Any(u => !u.Deleted && u.EnterpriseName == enterpriseName);
            return Json(!exists, JsonRequestBehavior.AllowGet);
        }

        [HttpGet] // 只能用GET ！！！
        public ActionResult CheckEnterpriseShortNameExists(string enterpriseShortName)
        {
            var exists = applicationDbContext.SysEnterprises.Any(u => !u.Deleted && u.EnterpriseShortName == enterpriseShortName);
            return Json(!exists, JsonRequestBehavior.AllowGet);
        }
    }
}
﻿using System.Collections.Generic;
using System.Web.Mvc;
using Eam.Core.Utility;
using EAM.Data.Domain;
using EAM.Data.Services;
using EAM.Data.Services.Query;
using System.Linq;
using Eam.Web.Portal.Areas.Account.Models;
using Newtonsoft.Json;
using Eam.Web.Portal.Areas.SysManage.Models;
using System;

namespace Eam.Web.Portal.Areas.Account.Controllers
{
    public class DeskTopController : EamAdminController
    {
        /// <summary>
        /// 系统信息服务
        /// </summary>
        private readonly ISysInfoService _sysInfoService;
        private readonly ISysWarningService _sysWarningService;

        public readonly IClassCodeServices _classCodeService;

        public readonly IAssetsService _AssetsService;

        public DeskTopController(ISysInfoService sysInfoService,
            ISysWarningService sysWarningService,
            IClassCodeServices classCodeServices,
            IAssetsService assetsService)
        {
            _sysInfoService = sysInfoService;
            _sysWarningService = sysWarningService;
            _classCodeService = classCodeServices;
            _AssetsService = assetsService;
        }

        private void BuildNode(List<ClassCode> source, ClassCodeTreeNode root, string parent)
        {
            var items = source.Where(c => c.ParentId == parent).ToList();
            root.isParent = items.Count > 0;
            foreach (var item in items)
            {
                var node = new ClassCodeTreeNode
                {
                    open = false,
                    name = item.ClassName,
                    id = item.EntityId,
                    pId = parent,
                    unit = item.Unit
                };
                root.children.Add(node);
                source.Remove(item);
                BuildNode(source, node, item.EntityId);
            }
        }

        public ActionResult Index()
        {
            /*var allClass = _classCodeService.GetAll();
            var root = new ClassCodeTreeNode();
            BuildNode(allClass, root, "");
            var classJson = JsonConvert.SerializeObject(root.children);
            Caching.Set("CLASS_CODE_JSON", classJson);*/
            var model = _AssetsService.GetAssetsGeneral();
            //2017-06-08 wnn

            List<AssetsMain> echars = _AssetsService.GetEchars();


            var echarsKey = "";//柱状图x轴'1月', '2月', '3月', '4月', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月'
            var echarsValue = "";//柱状图y轴500, 620, 480, 690, 953, 630, 710, 823, 942, 1021, 1190, 1249
            var maxNum = 0;
            int totalcount = 0;
            var count = 0;
            if (echars != null)
            {
                var tempkey = echars[0].PostingDate.ToString("yyyy/MM");
                foreach (var item in echars)
                {
                    var date = item.PostingDate.ToString("yyyy/MM");
                    string year = DateTime.Now.Year.ToString();
                    string judgeyear = "";
                    int yearcount = Convert.ToInt32(year);
                    while (!date.Contains(yearcount.ToString()))
                    {
                        yearcount--;
                        if (yearcount == 1900) {
                            break;
                        }
                    };
                    judgeyear = yearcount.ToString();
                    if (date.Contains(judgeyear))
                    {
                        if (!tempkey.Equals(date))
                        {
                            if (count < 11) {
                                echarsKey = "'" + item.PostingDate.ToString("yyyy/MM") + "', " + echarsKey;
                                echarsValue = totalcount.ToString() + "," + echarsValue;
                            }
                            tempkey = item.PostingDate.ToString("yyyy/MM");
                            totalcount = item.Counts;
                            count++;
                            if (maxNum < totalcount) { maxNum = totalcount; }
                        }
                        else
                        {
                            totalcount += item.Counts;
                        }
                        
                    }
                    
                    //echarsKey = "'" + item.PostingDate.ToString("yyyy/MM") + "', " + echarsKey;
                    //echarsValue = item.Counts.ToString() + "," + echarsValue;
                    //if (maxNum < item.Counts) { maxNum = item.Counts; }
                }
            }
            echarsKey = echarsKey + "'" + echars[0].PostingDate.ToString("yyyy/MM") + "' ";
            echarsValue = totalcount.ToString() + "," + echarsValue;
            echarsKey = echarsKey.Trim(',');
            echarsValue = echarsValue.Trim(',');

            ViewBag.echarsKey = echarsKey;
            ViewBag.echarsValue = echarsValue;
            ViewBag.maxNum = maxNum;//柱状图y最大值

            ViewBag.interval = maxNum / 5;
            ViewBag.role = Session["power"];

            return View(model);
        }

        /// <summary>
        /// 获取整个资产类别树
        /// </summary>
        /// <returns></returns>

        public ActionResult GetClassCodeTree()
        {
            var allClass = _classCodeService.GetAll();



            var root = new ClassCodeTreeNode();
            BuildNode(allClass, root, "");
            return Json(root.children, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取整个资产类别树
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //public ActionResult GetDeptCodeTree()
        //{
        //    var allClass = _classCodeService.GetAll();

        //    var allDept = 

        //    var root = new ClassCodeTreeNode();
        //    BuildNode(allClass, root, "");
        //    return Json(root.children, JsonRequestBehavior.AllowGet);
        //}


        /// <summary>
        /// 获取单个资产类别
        /// </summary>
        /// <param name="catCode"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetClassCode(string catCode)
        {
            var classCode = _classCodeService.Get(catCode);
            if (null == classCode)
                return Json("", JsonRequestBehavior.AllowGet);
            var nextNum = string.Format("{0}{1}", catCode.PadRight(9, '0'),
                 classCode.NextNum.ToString().PadLeft(6, '0'));
            return Json(nextNum, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取类型的列表以供前端 联想输入
        /// </summary>
        /// <returns></returns>
        public JsonResult GetClassCodeList()
        {
            JsonResult result = new JsonResult();
            var classCode = _classCodeService.GetAll();
            result.Data = classCode.Select(p => new
            {
                id = string.Format("{0}{1}", p.EntityId.PadRight(9, '0'), p.NextNum.ToString().PadLeft(6, '0')),
                text = string.Format("{0}{1}", p.EntityId.PadRight(9, '0'), p.NextNum.ToString().PadLeft(6, '0')) + p.ClassName
            });
            return result;
        }
        /// <summary>
        /// 系统信息表查询
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SysInfoQuery(AllRecordQuery model)
        {
            var result = _sysInfoService.QueryPage(model);
            return Json(result);
        }

        /// <summary>
        /// 系统信息表查询
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SysWarningQuery(AllRecordQuery model)
        {
            var result = _sysWarningService.QueryPage(model);
            return Json(result);
        }

        /// <summary>
        /// 资产类别表查询
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ClassCodeQuery(AllRecordQuery model)
        {
            var result = _classCodeService.QueryPage(model);
            return Json(result);
        }
    }
}

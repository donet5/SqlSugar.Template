﻿using DMSN.Common.BaseResult;
using DMSN.Common.Extensions.ExpressionFunc;
using DMSN.Common.Helper;
using SqlSugar;
using SqlSugar.Template.Contracts;
using SqlSugar.Template.Contracts.Param;
using SqlSugar.Template.Contracts.Result;
using SqlSugar.Template.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SqlSugar.Template.Service
{
    public class SysJobLogService : ISysJobLogService
    {
        public ISqlSugarClient db;
        public SysJobLogService(ISqlSugarClient sqlSugar)
        {
            db = sqlSugar;
        }
        /// <summary>
        /// 各种新增语法
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<ResponseResult> AddAsync(AddJobLogParam param)
        {
            ResponseResult result = new ResponseResult();
            if (param == null
                || string.IsNullOrEmpty(param.Name)
                || string.IsNullOrEmpty(param.Message))
            {
                result.errno = 1;
                result.errmsg = "参数错误";
                return result;
            }
            Sys_JobLog jobLogEntity = new Sys_JobLog()
            {
                Name = param.Name,
                JobLogType = param.JobLogType,
                ServerIP = IPHelper.GetCurrentIp(),
                TaskLogType = param.TaskLogType,
                Message = param.Message,
                CreateTime = DateTime.Now,
            };

            //插入返回自增列
            var flag = db.Insertable(jobLogEntity).ExecuteReturnIdentity();
            //插入返回影响行
            flag = await db.Insertable(jobLogEntity).ExecuteCommandAsync();
            //null 列不插入
            flag = await db.Insertable(jobLogEntity).IgnoreColumns(ignoreNullColumn: true).ExecuteCommandAsync();
            //插入指定列
            flag = db.Insertable(jobLogEntity).InsertColumns(it => new { it.Name, it.JobLogType }).ExecuteReturnIdentity();
            flag = db.Insertable(jobLogEntity).InsertColumns("Name", "JobLogType").ExecuteReturnIdentity();
            result.data = flag;
            return result;
        }
        /// <summary>
        /// 添加事物
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<ResponseResult> AddTranAsync(AddJobLogParam param)
        {
            ResponseResult result = new ResponseResult();
            if (param == null
                || string.IsNullOrEmpty(param.Name)
                || string.IsNullOrEmpty(param.Message))
            {
                result.errno = 1;
                result.errmsg = "参数错误";
                return result;
            }
            Sys_JobLog jobLogEntity = new Sys_JobLog()
            {
                Name = param.Name,
                JobLogType = param.JobLogType,
                ServerIP = IPHelper.GetCurrentIp(),
                TaskLogType = param.TaskLogType,
                Message = param.Message,
                CreateTime = DateTime.Now,
            };
            Sys_Log jobEntity = new Sys_Log()
            {
                Logger = "测试数据",
                Level = "测试等级",
                IP = "::",
                DeleteFlag = 0,
                LogType = 1,
                Message = "测试数据",
                SubSysID = 1,
                SubSysName = "测试子名称",
                Thread = "测试数据",
                Url = "http://www.yuxunwang.com/",
                MemberName = "18802727803",
                CreateTime = DateTime.Now,
                Exception = "测试异常信息",
            };
            db.Ado.UseTran(() =>
            {
                var t1 = db.Insertable(jobLogEntity).ExecuteCommandAsync();
                var t2 = db.Insertable(jobEntity).ExecuteCommandAsync();
            });

            return await Task.FromResult(result);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="jobLogID"></param>
        /// <returns></returns>
        public async Task<ResponseResult> DeleteAsync(long jobLogID)
        {
            ResponseResult result = new ResponseResult();
            if (jobLogID <= 0)
            {
                result.errno = 1;
                result.errmsg = "参数不合法";
                return result;
            }

            var t1 = await db.Deleteable<Sys_JobLog>()
                .Where(a => a.JobLogID == jobLogID)
                .ExecuteCommandAsync();
            result.data = t1;
            return result;
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="jobLogID"></param>
        /// <returns></returns>
        public async Task<ResponseResult> UpdateAsync(long jobLogID)
        {
            ResponseResult result = new ResponseResult();
            if (jobLogID <= 0)
            {
                result.errno = 1;
                result.errmsg = "参数不合法";
                return result;
            }
            var t1 = await db.Updateable(
                new Sys_JobLog()
                {
                    Message = "新标题",
                    CreateTime = DateTime.Now
                })
                .Where(a => a.JobLogID == jobLogID)
                .ExecuteCommandAsync();
            result.data = t1;
            return result;

        }
        /// <summary>
        /// 详情
        /// </summary>
        /// <param name="jobLogID"></param>
        /// <returns></returns>
        public async Task<ResponseResult<JobLogResult>> GetJobLogAsync(long jobLogID)
        {
            ResponseResult<JobLogResult> result = new ResponseResult<JobLogResult>() { data = new JobLogResult() };
            var entity = await db.Queryable<Sys_JobLog>()
                .Select<JobLogResult>()
                .FirstAsync(q => q.JobLogID == jobLogID);
            if (entity == null)
            {
                result.errno = 1;
                result.errmsg = "未找到相关数据";
                return result;
            }
            result.data = entity;
            return result;
        }
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="jobLogType"></param>
        /// <returns></returns>
        public async Task<ResponseResult<List<JobLogResult>>> GetJobLogListAsync(long jobLogType)
        {
            ResponseResult<List<JobLogResult>> result = new ResponseResult<List<JobLogResult>>()
            {
                data = new List<JobLogResult>()
            };
            if (jobLogType <= 0)
            {
                result.errno = 1;
                result.errmsg = "参数不合法";
                return result;
            }
            var list = await db.Queryable<Sys_JobLog>()
                .Where(q => q.JobLogType == jobLogType)
                .Select<JobLogResult>()
                .ToListAsync();
            if (list == null || list.Count <= 0)
            {
                result.errno = 2;
                result.errmsg = "未找到相关数据";
                return result;
            }
            result.data = list;
            return result;
        }
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<ResponsePageResult<JobLogResult>> SearchJobLogAsync(SearchJobLogParam param)
        {
            ResponsePageResult<JobLogResult> result = new ResponsePageResult<JobLogResult>()
            {
                data = new DataResultList<JobLogResult>()
            };
            if (param == null || param.JobLogType <= 0)
            {
                result.errno = 1;
                result.errmsg = "参数不合法";
                return result;
            }

            RefAsync<int> totalCount = 0;
            var expression = Expressionable.Create<Sys_JobLog>();
            expression.And(m => m.JobLogType == 1);
            Expression<Func<Sys_JobLog, bool>> where = expression.ToExpression();
            var list = await db.Queryable<Sys_JobLog>()
                .WhereIF(where != null, where)
                .OrderBy(q => q.JobLogID, OrderByType.Desc)
                .Select<JobLogResult>()
                .ToPageListAsync(param.PageIndex, param.PageSize, totalCount);
            if (list == null || list.Count <= 0)
            {
                result.errno = 2;
                result.errmsg = "未找到相关数据";
                return result;
            }
            result.data.ResultList = list;
            result.data.PageIndex = param.PageIndex;
            result.data.PageSize = param.PageSize;
            result.data.TotalRecord = (int)totalCount;
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Task<ResponseResult> Add2Async(AddJobLogParam param)
        {
            throw new NotImplementedException();
        }
    }
}

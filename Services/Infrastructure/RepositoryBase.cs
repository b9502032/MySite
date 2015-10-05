﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IServices.ISysServices;
using Models;

namespace Services.Infrastructure
{
    public abstract class RepositoryBase<T> where T : class
    {
        private readonly ApplicationDb _dataContext;
        private readonly IDbSet<T> _dbset;
        private readonly IUserInfo _userInfo;

        protected RepositoryBase(IDatabaseFactory databaseFactory, IUserInfo userInfo)
        {
            _dataContext = databaseFactory.Get();
            _userInfo = userInfo;
            _dbset = _dataContext.Set<T>();
        }

        /// <summary>
        ///     添加
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Add(T entity)
        {
            _dbset.Add(entity);
        }

        /// <summary>
        ///     更新
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Update(T entity)
        {
            _dbset.Attach(entity);
            _dataContext.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        ///     添加或者更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        public virtual void Save(Guid? id, T entity)
        {
            var ientity = entity as IDbSetBase;
            if (ientity != null)
            {
                //ientity.EnterpriseId = _userInfo.EnterpriseId;
                ientity.UserId = _userInfo.UserId;

                if (id.HasValue)
                {
                    Update(ientity as T);
                }
                else
                {
                    Add(ientity as T);
                }
            }
            else
            {
                if (id.HasValue)
                {
                    Update(entity);
                }
                else
                {
                    Add(entity);
                }
            }
        }

        /// <summary>
        ///     标记删除
        /// </summary>
        /// <param name="id"></param>
        public virtual void Delete(Guid id)
        {
            T item = GetById(id);
            Delete(item);
        }

        /// <summary>
        ///     标记删除
        /// </summary>
        /// <param name="item"></param>
        public virtual void Delete(T item)
        {
            var entity = item as IDbSetBase;
            //if (entity != null && entity.EnterpriseId.Equals(_userInfo.EnterpriseId))
            //    entity.Deleted = true;
            if (entity != null)
                entity.Deleted = true;
        }

        /// <summary>
        ///     标记删除
        /// </summary>
        /// <param name="where"></param>
        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            foreach (T item in GetAll(where))
            {
                Delete(item);
            }
        }

        /// <summary>
        ///     物理删除
        /// </summary>
        /// <param name="item"></param>
        public virtual void Remove(T item)
        {
            _dbset.Remove(item);
        }

        /// <summary>
        /// Select By Id Column
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T GetById(Guid id)
        {
            return _dbset.Find(id);
        }

        /// <summary>
        ///     获取全部企业数据
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> GetAllEnt()
        {
            return GetAllEnt(false);
        }

        /// <summary>
        /// 获取全部企业数据
        /// </summary>
        /// <param name="deleted"></param>
        /// <returns></returns>
        public virtual IQueryable<T> GetAllEnt(bool deleted)
        {
            // make one parameter c
            ParameterExpression param = Expression.Parameter(typeof (T), "c");
            // c.Deleted
            Expression left = Expression.Property(param, "Deleted");
            // c.CreatedDate
            Expression createdDate = Expression.Property(param, "CreatedDate");
            // deleted
            Expression right = Expression.Constant(deleted);
            // c.Deleted == deleted
            Expression filter = Expression.Equal(left, right);
            // c => c.Deleted == deleted
            Expression<Func<T, bool>> _predictor = Expression.Lambda<Func<T, bool>>(filter, param);
            // c => c.CreatedDate
            Expression<Func<T, DateTime>> orderByDate =
                Expression.Lambda<Func<T, DateTime>>(createdDate, param);

            //Expression<Func<T, DateTime>> order =
            //    Expression.Lambda<Func<T, DateTime>>(Expression.Property(param, "CreatedDate"), param);

            return _dbset.Where(_predictor).OrderByDescending(orderByDate);
        }

        /// <summary>
        ///     获取用户所在企业数据
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> GetAll()
        {
            return GetAll(false);
        }

        /// <summary>
        /// 获取用户所在企业数据 
        /// </summary>
        /// <param name="_deleted"></param>
        /// <returns></returns>
        public virtual IQueryable<T> GetAll(bool _deleted)
        {
            //// make one parameter c
            //ParameterExpression param = Expression.Parameter(typeof (T), "c"); 
            //// c.EnterpriseId
            //Expression left = Expression.Property(param, "EnterpriseId"); 
            //// _userInfo.EnterpriseId
            //Expression right = Expression.Constant(_userInfo.EnterpriseId); 
            //// c.EnterpriseId == _userInfo.EnterpriseId
            //Expression _equation = Expression.Equal(left, right); 
            //// c => c.EnterpriseId == _userInfo.EnterpriseId
            //Expression<Func<T, bool>> _predictor = Expression.Lambda<Func<T, bool>>(_equation, param); 

            //return GetAllEnt(_deleted).Where(_predictor);
            return GetAllEnt(_deleted);
        }

        /// <summary>
        ///     获取符合条件的用户所在企业数据
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual IQueryable<T> GetAll(Expression<Func<T, bool>> where)
        {
            return GetAll().Where(where);
        }


        public int Commit()
        {
            return _dataContext.Commit();
        }

        public Task<int> CommitAsync()
        {
            return _dataContext.CommitAsync();
        }
    }
}
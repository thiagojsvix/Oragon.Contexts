﻿using AopAlliance.Intercept;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oragon.Contexts
{
	public abstract class AbstractContextAroundAdvice<ContextType, AttributeType> : IMethodInterceptor
		where AttributeType : AbstractContextAttribute
		where ContextType : AbstractContext<AttributeType>
	{
		#region Protected Properties

		protected abstract Func<AttributeType, bool> AttributeQueryFilter { get; }

        protected abstract string ContextStackListKey { get; }

        protected Stack<AbstractContext<AttributeType>> ContextStack
		{
			get
			{
				Stack<AbstractContext<AttributeType>> contextStack = Spring.Threading.LogicalThreadContext.GetData(this.ContextStackListKey) as Stack<AbstractContext<AttributeType>>;
				if (contextStack == null)
				{
					contextStack = new Stack<AbstractContext<AttributeType>>();
					Spring.Threading.LogicalThreadContext.SetData(ContextStackListKey, contextStack);
				}
				return contextStack;
			}
		}

		#endregion Protected Properties

		#region Public Methods

		public object Invoke(IMethodInvocation invocation)
		{
			IEnumerable<AttributeType> contextAttributes = this.GetContextAttributes(invocation);
            object returnValue = contextAttributes.Any() ? this.Invoke(invocation, contextAttributes) : invocation.Proceed();
            return returnValue;
		}

		#endregion Public Methods

		#region Protected Methods

		/// <summary>
		///     Obtém informações de persistência definidas nos métodos
		/// </summary>
		/// <param name="invocation"></param>
		/// <returns></returns>
		protected IEnumerable<AttributeType> GetContextAttributes(IMethodInvocation invocation)
		{
			//Recupera os atributos do método
			IEnumerable<AttributeType> returnValue = invocation.GetAttibutes<AttributeType>(this.AttributeQueryFilter);
			return returnValue;
		}

		protected abstract object Invoke(IMethodInvocation invocation, IEnumerable<AttributeType> contextAttributes);

		#endregion Protected Methods
	}
}
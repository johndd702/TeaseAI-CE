﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TeaseAI_CE.Scripting
{
	/// <summary> Operators that can be used with variables. </summary>
	public enum Operators
	{
		// Math
		Add,
		Subtract,
		Multiply,
		Divide,
		// Logic
		Equal,
		More,
		Less,
		And,
		Or,
		//
		Assign,
	}

	/// <summary>
	/// The base variable class, should be used for [bool, float, string]
	/// by-reference thread-safe
	/// </summary>
	public class Variable
	{
		protected object _value = null;
		public object Value { get { return getObj(); } set { setObj(value); } }
		/// <summary> Does variable have a value? </summary>
		public virtual bool IsSet { get { return _value != null; } }

		/// <summary> If true scripts can only read. </summary>
		public bool Readonly = false;

		public Variable() { }
		public Variable(string value)
		{ _value = value; }
		public Variable(bool value)
		{ _value = value; }
		public Variable(float value)
		{ _value = value; }

		protected virtual object getObj()
		{
			return _value;
		}
		protected virtual void setObj(object value)
		{
			Interlocked.Exchange(ref _value, value);
		}

		public override string ToString()
		{
			if (IsSet)
				return Value.ToString();
			return "UnSet";
		}

		public static Variable Evaluate(BlockScope sender, Variable left, Operators op, Variable right)
		{
			var log = sender.Root.Log;
			bool validating = sender.Root.Valid == BlockBase.Validation.Running;
			if (left == null || right == null)
			{
				log.Error("Cannnot evaluate a null variable!");
				return null;
			}

			object l = left.Value;
			object r = right.Value;

			if (op == Operators.Assign)
			{
				if (!right.IsSet)
				{
					log.Error("Tried to set a variable to a unset variable.");
					return null;
				}

				if (left.Readonly)
				{
					log.Error(string.Format("Tried to assign to readonly variable."));
					return left;
				}

				if (left.IsSet)
				{
					if (l.GetType() == r.GetType())
					{
						if (!validating) // Don't change variable if we are validating.
							left.Value = r;
					}
					else
					{
						log.Error(string.Format("Tried to set a variable of type '{0}' to type '{1}'", l.GetType().Name, r.GetType().Name));
						return null;
					}
				}
				else
				{
					if (validating)
						left.Value = getDefault(right.Value.GetType());
					else
						left.Value = right.Value;
				}
				return left;
			}

			if (!left.IsSet || !right.IsSet)
			{
				log.Error("Cannot apply operator '" + op.ToString() + "' with unset variables!");
				return null;
			}

			switch (op)
			{
				// Math
				case Operators.Add:
					if (l is float && r is float)
						return new Variable((float)l + (float)r);
					else if (l is string && r is string)
						return new Variable(string.Concat(l, r));
					log.Error(string.Format("Unable to {0} {1} with {2}", op.ToString(), l.GetType().Name, r.GetType().Name));
					return null;
				case Operators.Subtract:
					if (l is float && r is float)
						return new Variable((float)l - (float)r);
					else if (l is string && r is string)
						return new Variable(((string)l).Replace((string)r, ""));
					log.Error(string.Format("Unable to {0} {1} with {2}", op.ToString(), l.GetType().Name, r.GetType().Name));
					return null;
				case Operators.Multiply:
					if (l is float && r is float)
						return new Variable((float)l * (float)r);
					log.Error(string.Format("Unable to {0} {1} with {2}", op.ToString(), l.GetType().Name, r.GetType().Name));
					return null;
				case Operators.Divide:
					if (l is float && r is float)
					{
						if (validating)
							return new Variable(default(float));
						float fl = (float)l;
						float fr = (float)r;
						if (fr == 0)
						{
							log.Warning("Tried to devide by zero!");
							fr = 1;
						}
						return new Variable(fl / fr);
					}
					log.Error(string.Format("Unable to {0} {1} with {2}", op.ToString(), l.GetType().Name, r.GetType().Name));
					return null;
				// Logic
				case Operators.Equal:
					if (l is string && r is string) // for strings we want to ignore the case.
						return new Variable((l as string).Equals((string)r, StringComparison.InvariantCultureIgnoreCase));
					return new Variable(l.Equals(r));
				case Operators.More:
					if (l is float && r is float)
						return new Variable((float)l > (float)r);
					log.Error(string.Format("Unable to {0} {1} with {2}", op.ToString(), l.GetType().Name, r.GetType().Name));
					return null;
				case Operators.Less:
					if (l is float && r is float)
						return new Variable((float)l < (float)r);
					log.Error(string.Format("Unable to {0} {1} with {2}", op.ToString(), l.GetType().Name, r.GetType().Name));
					return null;
				case Operators.And:
					if (l is bool && r is bool)
						return new Variable((bool)l && (bool)r);
					log.Error(string.Format("Unable to {0} {1} with {2}", op.ToString(), l.GetType().Name, r.GetType().Name));
					return null;
				case Operators.Or:
					if (l is bool && r is bool)
						return new Variable((bool)l || (bool)r);
					log.Error(string.Format("Unable to {0} {1} with {2}", op.ToString(), l.GetType().Name, r.GetType().Name));
					return null;
			}
			log.Error("Invalid operator: " + op.ToString());
			return null;
		}
		private static object getDefault(Type type)
		{
			try
			{
				if (type.IsValueType)
					return Activator.CreateInstance(type);
			}
			catch { }
			return null;
		}
	}

	/// <summary>
	/// Generic variable, should be used for class typed values.
	/// thread-safe, class T may not be thread-safe.
	/// </summary>
	public class Variable<T> : Variable where T : class
	{
		private new T _value = null;
		public new T Value
		{
			get { return _value; }
			set
			{ Interlocked.Exchange(ref _value, value); }
		}
		public override bool IsSet { get { return _value != null; } }

		public Variable(T value)
		{
			_value = value;
		}

		protected override object getObj()
		{
			return _value;
		}
		protected override void setObj(object value)
		{
			Interlocked.Exchange(ref _value, value as T);
		}

	}
}

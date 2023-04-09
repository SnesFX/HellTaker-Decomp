using System;
using System.Collections.Generic;

namespace UnityEngine.Timeline
{
	internal class IntervalTree<T> where T : IInterval
	{
		internal struct Entry
		{
			public long intervalStart;

			public long intervalEnd;

			public T item;
		}

		private const int kMinNodeSize = 10;

		private const int kInvalidNode = -1;

		private const long kCenterUnknown = long.MaxValue;

		private readonly List<Entry> m_Entries = new List<Entry>();

		private readonly List<IntervalTreeNode> m_Nodes = new List<IntervalTreeNode>();

		public bool dirty { get; internal set; }

		public void Add(T item)
		{
			if (item != null)
			{
				m_Entries.Add(new Entry
				{
					intervalStart = item.intervalStart,
					intervalEnd = item.intervalEnd,
					item = item
				});
				dirty = true;
			}
		}

		public void IntersectsWith(long value, List<T> results)
		{
			if (m_Entries.Count != 0)
			{
				if (dirty)
				{
					Rebuild();
					dirty = false;
				}
				if (m_Nodes.Count > 0)
				{
					Query(m_Nodes[0], value, results);
				}
			}
		}

		public void IntersectsWithRange(long start, long end, List<T> results)
		{
			if (start <= end && m_Entries.Count != 0)
			{
				if (dirty)
				{
					Rebuild();
					dirty = false;
				}
				if (m_Nodes.Count > 0)
				{
					QueryRange(m_Nodes[0], start, end, results);
				}
			}
		}

		public void UpdateIntervals()
		{
			bool flag = false;
			for (int i = 0; i < m_Entries.Count; i++)
			{
				Entry entry = m_Entries[i];
				long intervalStart = entry.item.intervalStart;
				long intervalEnd = entry.item.intervalEnd;
				flag |= entry.intervalStart != intervalStart;
				flag |= entry.intervalEnd != intervalEnd;
				m_Entries[i] = new Entry
				{
					intervalStart = intervalStart,
					intervalEnd = intervalEnd,
					item = entry.item
				};
			}
			dirty |= flag;
		}

		private void Query(IntervalTreeNode intervalTreeNode, long value, List<T> results)
		{
			for (int i = intervalTreeNode.first; i <= intervalTreeNode.last; i++)
			{
				Entry entry = m_Entries[i];
				if (value >= entry.intervalStart && value < entry.intervalEnd)
				{
					results.Add(entry.item);
				}
			}
			if (intervalTreeNode.center != long.MaxValue)
			{
				if (intervalTreeNode.left != -1 && value < intervalTreeNode.center)
				{
					Query(m_Nodes[intervalTreeNode.left], value, results);
				}
				if (intervalTreeNode.right != -1 && value > intervalTreeNode.center)
				{
					Query(m_Nodes[intervalTreeNode.right], value, results);
				}
			}
		}

		private void QueryRange(IntervalTreeNode intervalTreeNode, long start, long end, List<T> results)
		{
			for (int i = intervalTreeNode.first; i <= intervalTreeNode.last; i++)
			{
				Entry entry = m_Entries[i];
				if (end >= entry.intervalStart && start < entry.intervalEnd)
				{
					results.Add(entry.item);
				}
			}
			if (intervalTreeNode.center != long.MaxValue)
			{
				if (intervalTreeNode.left != -1 && start < intervalTreeNode.center)
				{
					QueryRange(m_Nodes[intervalTreeNode.left], start, end, results);
				}
				if (intervalTreeNode.right != -1 && end > intervalTreeNode.center)
				{
					QueryRange(m_Nodes[intervalTreeNode.right], start, end, results);
				}
			}
		}

		private void Rebuild()
		{
			m_Nodes.Clear();
			m_Nodes.Capacity = m_Entries.Capacity;
			Rebuild(0, m_Entries.Count - 1);
		}

		private int Rebuild(int start, int end)
		{
			IntervalTreeNode value = default(IntervalTreeNode);
			if (end - start + 1 < 10)
			{
				IntervalTreeNode intervalTreeNode = default(IntervalTreeNode);
				intervalTreeNode.center = long.MaxValue;
				intervalTreeNode.first = start;
				intervalTreeNode.last = end;
				intervalTreeNode.left = -1;
				intervalTreeNode.right = -1;
				value = intervalTreeNode;
				m_Nodes.Add(value);
				return m_Nodes.Count - 1;
			}
			long num = long.MaxValue;
			long num2 = long.MinValue;
			for (int i = start; i <= end; i++)
			{
				Entry entry = m_Entries[i];
				num = Math.Min(num, entry.intervalStart);
				num2 = Math.Max(num2, entry.intervalEnd);
			}
			long num3 = (value.center = (num2 + num) / 2);
			int num4 = start;
			int num5 = end;
			while (true)
			{
				if (num4 <= end && m_Entries[num4].intervalEnd < num3)
				{
					num4++;
					continue;
				}
				while (num5 >= start && m_Entries[num5].intervalEnd >= num3)
				{
					num5--;
				}
				if (num4 > num5)
				{
					break;
				}
				Entry value2 = m_Entries[num4];
				Entry value3 = m_Entries[num5];
				m_Entries[num5] = value2;
				m_Entries[num4] = value3;
			}
			value.first = num4;
			num5 = end;
			while (true)
			{
				if (num4 <= end && m_Entries[num4].intervalStart <= num3)
				{
					num4++;
					continue;
				}
				while (num5 >= start && m_Entries[num5].intervalStart > num3)
				{
					num5--;
				}
				if (num4 > num5)
				{
					break;
				}
				Entry value4 = m_Entries[num4];
				Entry value5 = m_Entries[num5];
				m_Entries[num5] = value4;
				m_Entries[num4] = value5;
			}
			value.last = num5;
			m_Nodes.Add(default(IntervalTreeNode));
			int num6 = m_Nodes.Count - 1;
			value.left = -1;
			value.right = -1;
			if (start < value.first)
			{
				value.left = Rebuild(start, value.first - 1);
			}
			if (end > value.last)
			{
				value.right = Rebuild(value.last + 1, end);
			}
			m_Nodes[num6] = value;
			return num6;
		}

		public void Clear()
		{
			m_Entries.Clear();
			m_Nodes.Clear();
		}
	}
}

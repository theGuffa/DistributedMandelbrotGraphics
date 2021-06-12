using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedMandelbrotGraphics.Classes {

	public class RingBuffer<T> : IEnumerable<T> {

		private T[] _buffer;
		private int _size;
		private int _first;
		private int _last;

		public int Length { get; private set; }

		public RingBuffer(int size) {
			_size = size;
			_buffer = new T[size];
			_first = 0;
			_last = -1;
			Length = 0;
		}

		public void Add(T value) {
			_last = (_last + 1) % _size;
			_buffer[_last] = value;
			if (Length == _size) {
				_first = (_first + 1) % _size;
			}
			Length++;
		}

		public IEnumerator<T> GetEnumerator() {
			for (int i = 0; i < Length; i++) {
				yield return _buffer[(_first + i) % _size];
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return _buffer.GetEnumerator();
		}

	}

}

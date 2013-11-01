using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LZSS
{
    class LZSS/*<TDataType> where TDataType : IComparable<TDataType>*/
    {
        public float Procent = 0f;

        /// <summary>
        /// Высвобождает память
        /// </summary>
        public void Clear()
        {
            Procent = 0f;
        }

        /// <summary>
        /// Выполняет поиск указанной последовательности в словаре
        /// </summary>
        /// <param name="dictionary">Входной массив/список в котором выполняем поиск подпоследовательности</param>
        /// <param name="input">Подпоследовательность для поиска </param>
        /// <returns>Если последовательность была найдена возвращает её индекс, иначе  -1</returns>
        private Int32 SearchInDict(IList<Byte> dictionary, IList<Byte> input)
        {
            //if (input.Count > dictionary.Count) return -1;
            for (var i = 0; i < dictionary.Count; i++)
            {
                for (var j = 0; j < input.Count && i + j < dictionary.Count; j++)
                {
                    if (dictionary[i + j] == input[j])
                    {
                        if (j + 1 == input.Count)
                            return i;
                    }
                    else break;
                }
            }
            return -1;
        }

        IEnumerable<Boolean> getBitList(Byte input)
        {
            return new BitArray(new[] { input }).Cast<Boolean>().ToList();
        }

        private Byte BitArrayToByte(BitArray ba)
        {
            Byte result = 0;
            for (Byte index = 0, m = 1; index < 8; index++, m *= 2)
                result += ba.Get(index) ? m : (Byte)0;
            return result;
        }


        /// <summary>
        /// Сжимает входную последовательность с помощью алгоритма LZSS
        /// </summary>
        /// <param name="source">Исходный поток данных для сжатия</param>
        /// <returns></returns>
        public BitArray Compress(Byte[] source)
        {
            //Словарь
            var dictionary = new List<Byte>();
            //Выходной поток
            var output = new List<Boolean>();
            //Буферное окошко
            var buffer = new List<Byte>();

            for (var i = 0; i < source.Length; i++)
            {
                buffer.Add(source[i]);
                while ((SearchInDict(dictionary, buffer) != -1 && i + 1 < source.Length))
                {
                    buffer.Add(source[++i]);
                }
                if (buffer.Count > 1)
                {
                    buffer.RemoveAt(buffer.Count - 1);
                    --i;
                }
                if (buffer.Count > 1)
                {
                    output.Add(true);
                    output.AddRange(getBitList((Byte)((dictionary.Count) - SearchInDict(dictionary, buffer))));
                    output.AddRange(getBitList((Byte)buffer.Count));
                    dictionary.AddRange(buffer);
                    while (dictionary.Count > 255)
                    {
                        dictionary.RemoveAt(0);
                    }
                    buffer.Clear();
                }
                else
                {
                    output.Add(false);
                    output.AddRange(new BitArray(buffer.ToArray()).Cast<Boolean>().ToList());
                    dictionary.AddRange(buffer);
                    while (dictionary.Count > 255)
                    {
                        dictionary.RemoveAt(0);
                    }
                    buffer.Clear();
                }
                Procent = (100f / source.Length) * i;
            }
            Procent = 100;
            var countBits = new BitArray(BitConverter.GetBytes(output.Count)).Cast<Boolean>().ToList();
            output.InsertRange(0, countBits);
            return new BitArray(output.ToArray());
        }

        /// <summary>
        /// Расжимает входную последовательность
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bitsCount">Колличество бит в исходном файле (за исключением мусора)</param>
        /// <returns></returns>
        public Byte[] UnCompress(BitArray source, Int32 bitsCount)
        {
            //Выходной поток
            var output = new List<Byte>();
            var tempByte = new BitArray(8);
            var bitOffset = new BitArray(8);
            var bitCount = new BitArray(8);
            for (var i = 32; i < bitsCount + 24; )
            {
                if (source[i] == false)
                {
                    for (var j = 0; j < 8 && j + i + 1 < source.Length; j++)
                    {
                        tempByte[j] = source[++i];
                    }
                    output.Add(BitArrayToByte(tempByte));
                    i++;
                }
                else
                {
                    for (var j = 0; j < 8; j++)
                    {
                        bitOffset[j] = source[++i];
                    }
                    for (var j = 0; j < 8; j++)
                    {
                        bitCount[j] = source[++i];
                    }
                    var offset = BitArrayToByte(new BitArray(bitOffset));
                    var count = BitArrayToByte(new BitArray(bitCount));
                    var dicCount = output.Count;
                    for (var c = 0; c < count; c++)
                    {
                        output.Add(output[dicCount - offset + c]);
                    }
                    i++;
                }
                Procent = (100f / source.Length) * i;
            }
            Procent = 100;
            return output.ToArray();
        }
    }
}

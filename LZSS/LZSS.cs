using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LZSS
{
    /// <summary>
    /// Очень тормознуто и медленно, надо бы переписать... Но делаллось исключительно как лаба :D
    /// </summary>
    class LZSS
    {
        public float Procent;

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
        private Int32 SearchInDict(IList<Byte> dictionary, IList<Byte> input, Int32 oldInd)
        {
            //if (input.Count > dictionary.Count) return -1;
            for (var i = oldInd; i < dictionary.Count; i++)
            {
                for (byte j = 0; j < input.Count && i + j < dictionary.Count; j++)
                {
                    if (dictionary[i + j] == input[j])
                    {
                        if (j + 1 == input.Count)
                        {
                            return i;
                        }
                    }
                    else break;
                }
            }
            return -1;
        }

        IEnumerable<Boolean> getBitList(Byte input)
        {
            var arr = new Boolean[8];
            for (Byte i = 0; i < 8; i++)
            {
                arr[i] = (input & (1 << i)) > 0;
            }
            return arr;
        }

        /// <summary>
        /// Сжимает входную последовательность с помощью алгоритма LZSS
        /// </summary>
        /// <param name="source">Исходный поток данных для сжатия</param>
        /// <returns></returns>
        public BitArray Compress(IList<Byte> source)
        {
            //Словарь
            var dictionary = new List<Byte>(255);
            //Выходной поток
            var output = new List<Boolean>(source.Count / 5);
            //Буферное окошко
            var buffer = new List<Byte>(255);

            for (var i = 0; i < source.Count; i++)
            {
                buffer.Add(source[i]);
                var oldInd = 0;
                do
                {
                    oldInd = SearchInDict(dictionary, buffer, oldInd);
                    if (oldInd != -1 && i + 1 < source.Count)
                        buffer.Add(source[++i]);
                    else
                        break;
                } while (true);

                if (buffer.Count > 1)
                {
                    buffer.RemoveAt(buffer.Count - 1);
                    --i;
                }
                if (buffer.Count > 1)
                {
                    output.Add(true);
                    output.AddRange(getBitList((Byte)((dictionary.Count) - SearchInDict(dictionary, buffer, 0))));
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
                    output.AddRange(new BitArray(buffer.ToArray()).Cast<Boolean>());
                    dictionary.AddRange(buffer);
                    while (dictionary.Count > 255)
                    {
                        dictionary.RemoveAt(0);
                    }
                    buffer.Clear();
                }
                Procent = (100f / source.Count) * i;
            }
            Procent = 100;
            var countBits = new BitArray(BitConverter.GetBytes(output.Count)).Cast<Boolean>();
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
            for (var i = 32; i < bitsCount + 24;)
            {
                if (source[i] == false)
                {
                    Byte tempByte = 0x0;
                    for (byte j = 0; j < 8 && j + i + 1 < source.Length; j++)
                    {
                        tempByte |= (byte)((source[++i] ? 1 : 0) << j);
                    }
                    output.Add(tempByte);
                    i++;
                }
                else
                {
                    Byte offset = 0;
                    Byte count = 0;
                    for (byte j = 0; j < 8; j++)
                    {
                        offset |= (byte)((source[++i] ? 1 : 0) << j);
                    }
                    for (byte j = 0; j < 8; j++)
                    {
                        count |= (byte)((source[++i] ? 1 : 0) << j);
                    }
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

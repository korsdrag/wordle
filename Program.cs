using System.Diagnostics;

namespace WordlePuzzle
{
    internal class Program
    {
        public static List<string> wordList = new List<string>();
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var answers = File.ReadAllLines(@"D:\Git\projects\WordlePuzzle\words\wordle-nyt-answers-alphabetical.txt").ToList();
            var guesses = File.ReadAllLines(@"D:\Git\projects\WordlePuzzle\words\wordle-nyt-allowed-guesses.txt").ToList();

            answers.AddRange(guesses);
            wordList.AddRange(answers);

            Console.WriteLine(wordList.Count + " raw words");


            int[] cookedWords = wordList
                .ConvertAll(x => EncodeWord(x))
                .Where(x => SparseBitcount(x) == 5)
                .OrderBy(x => x)
                .Distinct()
                .ToArray();


            Console.WriteLine(cookedWords.Length + " pruned words\n");


            Parallel.For(0, cookedWords.Length, i =>
            {
                int A = cookedWords[i];

                for (int j = i + 1; j < cookedWords.Length; ++j)
                {
                    int B = cookedWords[j];
                    if ((A & B) != 0)
                        continue;
                    int AB = A | B;

                    for (int k = j + 1; k < cookedWords.Length; ++k)
                    {
                        int C = cookedWords[k];
                        if ((AB & C) != 0)
                            continue;
                        int ABC = AB | C;

                        for (int l = k + 1; l < cookedWords.Length; ++l)
                        {
                            int D = cookedWords[l];
                            if ((ABC & D) != 0)
                                continue;
                            int ABCD = ABC | D;

                            for (int m = l + 1; m < cookedWords.Length; ++m)
                            {
                                int E = cookedWords[m];

                                if ((ABCD & E) != 0)
                                    continue;

                                TimeSpan ts = stopwatch.Elapsed;
                                string elapsedTime = String.Format("[{0:00}:{1:00}:{2:00}]", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                                Console.WriteLine("{0}\n{1}\n\n", elapsedTime, DecodeWords(A, B, C, D, E));
                            }

                        }
                    }
                }
            });

            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("[{0:00}:{1:00}:{2:00}]", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine($"Total elapsed time: {elapsedTime}");
        }



        private static string DecodeWords(params int[] words)
        {
            var wordsList = words.ToList();
            return wordsList.ConvertAll(x => DecodeWord(x)).Aggregate((a, b) => a + '\n' + b);
        }

        private static string DecodeWord(int word)
        {
            var temp = wordList.Where(x => EncodeWord(x) == word).Aggregate((current, next) => current + "/" + next);
            return $"{VisualizeWord(word)} {temp}";
        }

        private static string VisualizeWord(int word)
        {
            //    1 1   1    1  1        
            // ---D-F---J----O--R--------
            char[] a = new char[26];
            word <<= 6;
            for (int i = 0; i < a.Length; ++i, word <<= 1)
            {
                a[i] = (word < 0) ? (char)('A' + i) : '-';
            }
            return new string(a);
        }

        public static int SparseBitcount(int n)
        {
            int count = 0;
            while (n != 0)
            {
                count++;
                n &= (n - 1);
            }
            return count;
        }


        public static int EncodeWord(string raw)
        {
            int bitset = 0;
            for (int i = 0; i < raw.Length; ++i)
            {
                bitset |= 1 << 26 >> raw[i];
            }
            return bitset;
        }
    }
}
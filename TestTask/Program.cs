using System;
using System.Collections.Generic;
using System.Linq;

namespace TestTask
{
    public class Program
    {

        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            IReadOnlyStream inputStream1 = GetInputStream(args[0]);
            IReadOnlyStream inputStream2 = GetInputStream(args[1]);

            IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
            IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);

            SelectCharStatsByType(ref singleLetterStats, CharType.Vowel);
            SelectCharStatsByType(ref doubleLetterStats, CharType.Consonants);

            PrintStatistic(singleLetterStats);
            PrintStatistic(doubleLetterStats);

            inputStream1.Close();
            inputStream2.Close();

            Console.ReadKey();
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            List<LetterStats> listLetterStats = new List<LetterStats>();

            stream.ResetPositionToStart();
            while (!stream.IsEof)
            {
                LetterStats letterStats;
                char c = stream.ReadNextChar();

                if (!char.IsLetter(c))
                    continue;

                if (listLetterStats.Any(l => l.Letter == c.ToString()))
                {
                    letterStats = listLetterStats.First(l => l.Letter == c.ToString());
                }
                else
                {
                    letterStats = new LetterStats
                    {
                        Letter = c.ToString(),
                        Count = 0
                    };
                    listLetterStats.Add(letterStats);
                }

                IncStatistic(letterStats);
            }

            return listLetterStats;
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой паре букв, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            List<LetterStats> listLetterStats = new List<LetterStats>();

            stream.ResetPositionToStart();
            if (stream.IsEof)
            {
                return listLetterStats;
            }
            char oldChar = stream.ReadNextChar();
            char newChar; 
            while (!stream.IsEof)
            {
                LetterStats letterStats;
                newChar = stream.ReadNextChar();
                if (oldChar.ToString().ToLower() ==  newChar.ToString().ToLower())
                {
                    if (listLetterStats.Any(l => l.Letter == $"{newChar}{newChar}"))
                    {
                        letterStats = listLetterStats.First(l => l.Letter == $"{newChar.ToString().ToLower()}{newChar.ToString().ToLower()}");
                    }
                    else
                    {
                        letterStats = new LetterStats
                        {
                            Letter = $"{newChar.ToString().ToLower()}{newChar.ToString().ToLower()}",
                            Count = 0
                        };
                        listLetterStats.Add(letterStats);
                    }
                    IncStatistic(letterStats);
                }

                oldChar = newChar;

            }

            return listLetterStats;
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void SelectCharStatsByType(ref IList<LetterStats> letters, CharType charType)
        {
            var vowels = "ёуеыаоэяиюeyuioa";
            var consonants = "цкнгшщзхъфвпрлджчсмтьбqwrtpsdfghjklzxcvbnm";
            List<LetterStats> finalList = new List<LetterStats>();
            switch (charType)
            {
                case CharType.Consonants:
                    letters = letters.Where(l => consonants.Contains(l.Letter.ToLower()[0])).ToList();
                    break;
                case CharType.Vowel:
                    letters = letters.Where(l => vowels.Contains(l.Letter.ToLower()[0])).ToList();
                    break;
            }
            
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            letters = letters.OrderBy(l => l.Letter).ToList();

            Console.WriteLine($"\nНачало метода {nameof(PrintStatistic)}");

            foreach (LetterStats letterStats in letters)
            {
                Console.WriteLine($"{letterStats.Letter} : {letterStats.Count}");
            }
            Console.WriteLine($"Итого: {letters.Sum(l => l.Count)}");
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(LetterStats letterStats)
        {
            letterStats.Count++;
        }
    }
}

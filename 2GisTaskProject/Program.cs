using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace _2GisTaskProject
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Для начала теста нажмите любую клавишу(esc пропуска Теста 1)...\r\n");
            if (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                Console.WriteLine("----------Тест №1. Функциональность DoubleDictionary----------");
                DictionaryTest1();
            }
            
            Console.WriteLine("Для продолжения нажмите любую клавишу(esc для пропуска Теста 2)...\r\n");
            if (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                Console.WriteLine("----------Тест №2. Скорость с пользовательским типом ключей на основе GUID--------");
                DictionaryTest2(new DoubleKeyDictionary<UserKey<Guid>, UserKey<Guid>, int>());
            }

            Console.WriteLine("Для продолжения нажмите любую клавишу(esc для пропуска Теста 3)...\r\n");
            if (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                Console.WriteLine("----------Тест №3. Скорость с типом ключей int--------");
                DictionaryTest3(new DoubleKeyDictionary<int, int, int>());
            }

            Console.WriteLine("Для продолжения нажмите любую клавишу(esc для пропуска Теста 4)...\r\n");
            if (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                Console.WriteLine("----------Тест №4. Данные теста №3 в нескольких потоках--------");
                ConcurrentTest();
            }
            
            Console.WriteLine("Тестирование окончено. Для выхода нажмите любую клавишу...");
            Console.ReadKey();
        }
        
        public static void DictionaryTest1()
        {            
            var dd = new DoubleKeyDictionary<string, UserKey<string>, int>
            {
                { "Ivan", new UserKey<string>("Ivanov"), 1000 },
                { "Ivan", new UserKey<string>("Kozlov"), 800 },
                { "Ivan", new UserKey<string>("Sukhov"), 450 },
                { "Peter", new UserKey<string>("Ivanov"), 1560 },
                { "Peter", new UserKey<string>("Kozlov"), 1200 },
                { "Peter", new UserKey<string>("Sukhov"), 0 }
            };
            //Перегруженный ToString()
            Console.WriteLine(dd);  

            var key1 = "Peter";
            var key2 = new UserKey<string>("Sukhov");
            var key = new DoubleKey<string, UserKey<string>>(key1, key2);

            Console.WriteLine(string.Join(", ", dd.GetValuesByKey1(key1)));
            Console.WriteLine(string.Join(", ", dd.GetValuesByKey2(key2)));
            
            //Для метода Add предпочел избежать обработки исключений. TryXXX
            if (!dd.Add(key, 100))
                Console.WriteLine($"Добавление значения с ключом {key.Key1} {key.Key2} не выполнено");

            Console.WriteLine($"{dd.RemoveByKey1(key1)} items with key1 = {key1} was removed!");
            Console.WriteLine($"{dd.RemoveByKey2(key2)} items with key2 = {key2.Key} was removed!");

            if (dd.Add(key, 100))
                Console.WriteLine($"Добавление значения с ключом {key.Key1} {key.Key2} выполнено");

            //индексаторы, исключения для наглядности
            Console.WriteLine($"{key.Key1}, {key.Key2}, {dd[key]}");
            try
            {
                key1 = "Ivan";
                Console.WriteLine($"{key1}, {key2.Key}, {dd[key1, key2]}");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"{e.Message} [{key1}, {key2.Key}]");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine(dd);
            Console.WriteLine("----изменяю значение по имеющемуся индексатору-------");
            dd[key] = 458;
            Console.WriteLine(dd);
            Console.WriteLine("------добавляю значение по отсутсвующему индексатору-------");
            dd["Alex", new UserKey<string>("Laptev")] = 4520;
            Console.WriteLine(dd);
        }
        public static void DictionaryTest2(DoubleKeyDictionary<UserKey<Guid>, UserKey<Guid>, int> dd,
            int s1 = 10000, 
            int s2 = 10, 
            int sshow = 245)
        {
            //Применяю универсальный пользовательский тип для обоих ключей, объекты которого сравниваются по значению
            
            Stopwatch sw = new Stopwatch();
            Random r = new Random();
            int err = 0;
            
            var testKey = new UserKey<Guid>(Guid.Empty);

            sw.Start();
            for (int i = s1; --i >= 0;)
            {
                var key1 = new UserKey<Guid>(Guid.NewGuid());
                //нужен для вывода части словаря (значение i произвольное)
                if (i == sshow) 
                    testKey = key1;

                //иногда цикл удобнее оформить на уменьшение..
                for (int j = s2; --j >= 0;)
                {
                    var key = new DoubleKey<UserKey<Guid>, UserKey<Guid>>(
                        key1,
                        new UserKey<Guid>(Guid.NewGuid()));
                    if (!dd.Add(key, r.Next(0, int.MaxValue)))
                        err++;
                }
            }
            sw.Stop();

            Console.WriteLine($"{Thread.CurrentThread.Name} - Выборка из набора ({s2} записей):");
            //для демонстрации работы с LINQ
            dd.GetItemsByKey1(testKey).Select(x =>
            {
                Console.WriteLine($"{Thread.CurrentThread.Name} - {x.Key.Key1}, {x.Key.Key2}, {x.Value}");
                return x;
            }).ToList();

            Console.WriteLine($"\r\n{Thread.CurrentThread.Name} - DoubleDictionary by length = {dd.Count}, errors = {err}, at {sw.Elapsed} was created.");
        }
        public static void DictionaryTest3(DoubleKeyDictionary<int, int, int> dd,
            int s1 = 1000,
            int s2 = 10,
            int sshow = 8)
        {
            Stopwatch sw = new Stopwatch();
            Random r = new Random();
            int err = 0;
            sw.Start();
            for (int i = s1; --i >= 0;)
            {
               for(int j = s2; --j >= 0;)
                if (!dd.Add(i, j, r.Next(0, s1*s2)))
                    err++;
            }
            sw.Stop();
            
            Console.WriteLine($"{Thread.CurrentThread.Name} - Выборка из набора ({s1*s2} записей):");
            dd.GetItemsByKey1(sshow < s1 ? sshow : 0).Select(x => 
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name} - {x.Key.Key1}, {x.Key.Key2}, {x.Value}");
                    return x;
                }).ToList();

            Console.WriteLine($"\r\n{Thread.CurrentThread.Name} - DoubleDictionary by length = {dd.Count}, errors = {err}, at {sw.Elapsed} was created.");
        }

        public static void ConcurrentTest()
        {
            //var dd = new DoubleKeyDictionary<UserKey<Guid>, UserKey<Guid>, int>();
            var dd = new DoubleKeyDictionary<int, int, int>();
            
            var threads = new Thread[Environment.ProcessorCount];
            for (int i = 0; i < threads.Length; i++)
            {
                var t = new Thread(() => DictionaryTest3(dd, i, i))
                {
                    Name = $"Thread {i}"
                };

                t.Start();
                
                threads[i] = t;
            }

            for (int i = 0; i < threads.Length; i++)
                threads[i].Join();

            Console.WriteLine(dd);
        }        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Gender.Data
{
    class Program
    {
        static void Main(string[] args)
        {
            string testDbPath = @"c:\Users\unbornchikken\Documents\x\Gender\T_gender_20x24.pdb.pat";
            string trainingDbPath = @"c:\Users\unbornchikken\Documents\x\Gender\L_gender_20x24.pdb.pat";
            int count = 0;
            int freq = 1000;
            using (var testFile = File.OpenRead(testDbPath))
            using (var trainingFile = File.OpenRead(trainingDbPath))
            using (var ctx = new GenderEntities())
            {
                var list = new LinkedList<Item>();
                ctx.DeleteAll();
                var testReader = new RawGenderItemReader(testFile, false);
                var trainingReader = new RawGenderItemReader(trainingFile, true);
                int all = testReader.ItemCount + trainingReader.ItemCount;
                foreach (var item in testReader.ReadItems().Concat(trainingReader.ReadItems()))
                {
                    ctx.AddToItems(item);
                    list.AddLast(item);
                    if (++count % freq == 0)
                    {
                        Console.WriteLine("{0} / {1}", count, all);
                        ctx.SaveChanges();
                        Detach(ctx, list);
                    }
                }
                ctx.SaveChanges();
            }
            Console.ReadKey();
        }

        private static void Detach(GenderEntities ctx, LinkedList<Item> list)
        {
            foreach (var item in list) ctx.Items.Detach(item);
            list.Clear();
        }
    }
}

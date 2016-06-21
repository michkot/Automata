﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;

using Microsoft.Automata.MSO;
using Microsoft.Automata;
using Microsoft.Automata.Z3;
using Microsoft.Automata.Z3.Internal;
using Microsoft.Z3;
using System.IO;
using Microsoft.Automata.MSO.Mona;
using System.Threading;

namespace MSO.Eval
{
    class LargeMinterm
    {
        static int kminterm = 40;
        static int maxmint = 19;
        static int numTests = 1;

        public static void MintermTest()
        {

            var sw = new Stopwatch();

            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"..\msomintermp1s1.txt"))
            {
                for (int size = 2; size < kminterm; size++)
                {
                    var solver = new CharSetSolver();
                    MSOFormula<BDD> phi = new MSOTrue<BDD>();

                    for (int k = 1; k < size; k++)
                    {
                        var leq = new MSOLt<BDD>(new Variable("x" + (k - 1), true), new Variable("x" + k, true));
                        phi = new MSOAnd<BDD>(phi, leq);

                    }
                    for (int k = 0; k < size; k++)
                    {
                        var axk = new MSOPredicate<BDD>(solver.MkBitTrue(k), new Variable("x" + k, true));
                        phi = new MSOAnd<BDD>(phi, axk);

                    }
                    for (int k = size - 1; k >= 0; k--)
                    {
                        phi = new MSOExists<BDD>(new Variable("x" + k, true), phi);
                    }

                    sw.Restart();
                    for (int t = 0; t < numTests; t++)
                    {
                        phi.GetAutomaton(solver);
                    }
                    sw.Stop();

                    var t1 = sw.ElapsedMilliseconds;

                    file.WriteLine((double)t1 / numTests);
                    Console.WriteLine((double)t1 / numTests);
                }
            }
            using (System.IO.StreamWriter file =
           new System.IO.StreamWriter(@"..\msomintermp2s1.txt"))
            {
                for (int size = 2; size < kminterm; size++)
                {

                    // Tsolve force
                    var solver = new CharSetSolver();

                    MSOFormula<BDD> phi = new MSOTrue<BDD>();

                    for (int k = 0; k < size; k++)
                    {
                        var axk = new MSOPredicate<BDD>(solver.MkBitTrue(k), new Variable("x" + k, true));
                        phi = new MSOAnd<BDD>(phi, axk);

                    }
                    for (int k = size - 1; k >= 0; k--)
                    {
                        phi = new MSOExists<BDD>(new Variable("x" + k, true), phi);
                    }

                    var t1 = 60000L;
                    if (size <= maxmint)
                    {
                        sw.Restart();
                        for (int t = 0; t < numTests; t++)
                        {
                            phi.GetAutomaton(solver);
                        }
                        sw.Stop();

                        t1 = sw.ElapsedMilliseconds;
                    }
                    file.WriteLine((double)t1 / numTests);
                    Console.WriteLine((double)t1 / numTests);
                }
            }
            using (System.IO.StreamWriter file =
           new System.IO.StreamWriter(@"..\msomintermp1s2.txt"))
            {
                for (int size = 2; size < kminterm; size++)
                {

                    // Tsolve solver 2
                    var solver = new CharSetSolver(BitWidth.BV64);

                    MSOFormula<BDD> phi = new MSOTrue<BDD>();

                    for (int k = 1; k < size; k++)
                    {
                        var leq = new MSOLt<BDD>(new Variable("x" + (k - 1), true), new Variable("x" + k, true));
                        phi = new MSOAnd<BDD>(phi, leq);

                    }
                    for (int k = 0; k < size; k++)
                    {
                        var axk = new MSOPredicate<BDD>(solver.MkBitTrue(k), new Variable("x" + k, true));
                        phi = new MSOAnd<BDD>(phi, axk);

                    }
                    for (int k = size - 1; k >= 0; k--)
                    {
                        phi = new MSOExists<BDD>(new Variable("x" + k, true), phi);
                    }

                    var t1 = 60000L;


                    sw.Restart();
                    for (int t = 0; t < numTests; t++)
                    {
                        phi.GetAutomaton(solver);
                    }
                    sw.Stop();

                    t1 = sw.ElapsedMilliseconds;

                    file.WriteLine((double)t1 / numTests);
                    Console.WriteLine((double)t1 / numTests);
                }
            }
            using (System.IO.StreamWriter file =
           new System.IO.StreamWriter(@"..\msomintermp2s2.txt"))
            {
                for (int size = 2; size < kminterm; size++)
                {

                    var solver = new CharSetSolver();
                    //Tforce sol 2
                    MSOFormula<BDD> phi = new MSOTrue<BDD>();

                    for (int k = 0; k < size; k++)
                    {
                        var axk = new MSOPredicate<BDD>(solver.MkBitTrue(k), new Variable("x" + k, true));
                        phi = new MSOAnd<BDD>(phi, axk);

                    }
                    for (int k = size - 1; k >= 0; k--)
                    {
                        phi = new MSOExists<BDD>(new Variable("x" + k, true), phi);
                    }

                    var t1 = 60000L;
                    if (size <= maxmint)
                    {
                        sw.Restart();
                        for (int t = 0; t < numTests; t++)
                        {
                            phi.GetAutomaton(solver);
                        }
                        sw.Stop();
                        t1 = sw.ElapsedMilliseconds;
                    }
                    file.WriteLine((double)t1 / numTests);
                    Console.WriteLine((double)t1 / numTests);
                }
            }
            using (System.IO.StreamWriter file =
           new System.IO.StreamWriter(@"..\msominterm.txt"))
            {
                for (int size = 2; size < kminterm; size++)
                {
                    //Tminterm
                    var solver = new CharSetSolver(BitWidth.BV64);
                    BDD[] predicates = new BDD[size];
                    solver.GenerateMinterms();
                    for (int k = 0; k < size; k++)
                        predicates[k] = solver.MkBitTrue(k);

                    var t1 = 60000L * numTests;
                    if (size <= maxmint)
                    {
                        sw.Restart();
                        for (int t = 0; t < numTests; t++)
                        {
                            var mint = solver.GenerateMinterms(predicates).ToList();
                        }
                        sw.Stop();
                        t1 = sw.ElapsedMilliseconds;
                    }

                    file.WriteLine((double)t1 / numTests);
                    Console.WriteLine((double)t1 / numTests);
                }
            }
        }


        public static void TestLargeLoris()
        {
            var max = 10;
            for (int i = 1; i < max; i++)
                TestMintermExplosion(i, true);
        }

        static void TestMintermExplosion(int bitWidth, bool useBDD = false)
        {

            Console.WriteLine("----------------");
            Console.WriteLine(bitWidth.ToString());

            if (useBDD)
            {
                var S = new CharSetSolver(BitWidth.BV7);

                Console.WriteLine("BDD");
                int t = System.Environment.TickCount;
                var aut1 = CreateAutomaton1<BDD>(S.MkBitTrue, bitWidth, S);
                var aut2 = CreateAutomaton2<BDD>(S.MkBitTrue, bitWidth, S);
                var aut3 = CreateAutomaton3<BDD>(S.MkBitTrue, bitWidth, S);
                t = System.Environment.TickCount - t;
                Console.WriteLine(t + "ms");
                //aut.ShowGraph("aut" + bitWidth);
            }
            else
            {
                Console.WriteLine("Z3");
                Z3Provider Z = new Z3Provider(BitWidth.BV7);
                var x = Z.MkConst("x", Z.IntSort);
                Func<int, Expr> f = (i => Z.MkEq((Z.MkInt(1)), Z.MkMod(Z.MkDiv(x, Z.MkInt(1 << (i % 32))), Z.MkInt(2))));
                int t = System.Environment.TickCount;
                var aut1 = CreateAutomaton1<Expr>(f, bitWidth, Z);
                var aut2 = CreateAutomaton2<Expr>(f, bitWidth, Z);
                var aut3 = CreateAutomaton3<Expr>(f, bitWidth, Z);
                t = System.Environment.TickCount - t;
                Console.WriteLine(t + "ms");
                //aut.ShowGraph("aut" + bitWidth);
            }
        }

        static Automaton<T> CreateAutomaton1<T>(Func<int, T> f, int bitWidth, IBooleanAlgebra<T> Z)
        {
            Func<int, string, MSOPredicate<T>> pred = (i, s) => new MSOPredicate<T>(f(i), new Variable(s, true));

            MSOFormula<T> phi = new MSOFalse<T>();

            for (int index = 0; index < bitWidth; index++)
            {
                var phi1 = pred(index, "var");
                phi = new MSOOr<T>(phi, phi1);
            }

            phi = new MSOExists<T>(new Variable("var", true), phi);

            var aut = phi.GetAutomaton(Z);

            return aut;
        }

        static Automaton<T> CreateAutomaton2<T>(Func<int, T> f, int bitWidth, IBooleanAlgebra<T> Z)
        {

            Func<int, string, MSOPredicate<T>> pred = (i, s) => new MSOPredicate<T>(f(i), new Variable(s, true));

            MSOFormula<T> phi = new MSOFalse<T>();

            for (int index = 0; index < bitWidth; index++)
            {
                MSOFormula<T> phi1 = pred(index, "var");
                phi1 = new MSOExists<T>(new Variable("var", true), phi1);
                phi = new MSOOr<T>(phi, phi1);
            }

            phi = new MSOExists<T>(new Variable("var", true), phi);

            var aut = phi.GetAutomaton(Z);

            return aut;
        }

        static Automaton<T> CreateAutomaton3<T>(Func<int, T> f, int bitWidth, IBooleanAlgebra<T> Z)
        {

            Func<int, string, MSOPredicate<T>> pred = (i, s) => new MSOPredicate<T>(f(i), new Variable(s, true));

            MSOFormula<T> phi = new MSOTrue<T>();

            // x1<x2<x3<x4...
            for (int index = 1; index < bitWidth; index++)
            {
                MSOFormula<T> phi1 = new MSOLt<T>(new Variable("x" + (index - 1), true), new Variable("x" + index, true));
                phi = new MSOAnd<T>(phi, phi1);
            }

            // bi(xi)
            for (int index = 0; index < bitWidth; index++)
            {
                MSOFormula<T> phi1 = pred(index, "x" + index);
                phi = new MSOAnd<T>(phi, phi1);
            }

            // exists forall...
            for (int index = 0; index < bitWidth; index++)
            {
                if (index % 2 == 0)
                    phi = new MSOExists<T>(new Variable("x" + index, true), phi);
                else
                    phi = new MSOForall<T>(new Variable("x" + index, true), phi);
            }

            var aut = phi.GetAutomaton(Z);
            return aut;
        }
    }
}

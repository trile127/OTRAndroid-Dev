using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;



namespace OTRAndroid.Crypto
{
    class DiffieHellman
    {

        ////Limit is 2^32, so 4,294,967,296
        //public static Java.Util.BitSet computePrimes(int limit)
        //{
        //    Java.Util.BitSet primes = new Java.Util.BitSet();
        //    primes.Set(0, false);
        //    primes.Set(1, false);
        //    primes.Set(2, limit, true);
        //    for (int i = 1000000000; i * i < limit; i++)
        //    {
        //        if (primes.Get(i))
        //        {
        //            for (int j = i * i; j < limit; j += i)
        //            {
        //                primes.Clear(j);
        //            }
        //        }
        //    }

        //    /* Primality test function used in expression. */
        //    Expression e1 = new Expression("ispr(5)");
        //    Expression e2 = new Expression("ispr(9)");

        //    /* Calculation and result output */
        //    mXparser.consolePrintln("Res 1: " + e1.getExpressionString() + " = " + e1.calculate());
        //    mXparser.consolePrintln("Res 2: " + e2.getExpressionString() + " = " + e2

        //    return primes;
        //}

        /**
    * Checks to see if the requested value is prime.
    */
        public  Boolean isPrime(int inputNum)
        {
            if (inputNum <= 3 || inputNum % 2 == 0)
                return inputNum == 2 || inputNum == 3; //this returns false if number is <=1 & true if number = 2 or 3
            int divisor = 3;
            while ((divisor <= Math.Sqrt(inputNum)) && (inputNum % divisor != 0))
                divisor += 2; //iterates through all possible divisors
            return inputNum % divisor != 0; //returns true/false
        }


        // Function to generate prime number p
        //public Int64 getPrime() {

        //    //var r io.Reader
        //    //var randomPrime* big.Int
        //    Int64 randomePrime;
        //    int randomPrimeInt;
        //    int error = 1;
        //    //var randomPrimeInt int
        //    //var err error
        //    System.Random r = new System.Random();
        //    // Generate as long as the result is a prime and not <nil>
        //    while (error == 1)
        //    {
        //        int num = r.Next();

        //    }
                


        // //           // Writing random number into io.Reader object r in order to pass it to rand.Prime
        // //           mrand.Seed(time.Now().UTC().UnixNano())

        // //       r = strings.NewReader(strconv.Itoa(mrand.Int()))
        // //       // 32 bit primes seem to be the best compromise between randomness and reliability
        // //           randomPrime, err = crand.Prime(r, 32)
        // //       // Do until there is no error anymore, then break and return prime number
        // //           if err == nil {
        // //               break

        // //       }
        // //       }
        // //       randomPrimeInt, _ = strconv.Atoi(randomPrime.String())
	       // //fmt.Println("********** Generate initial Diffie Hellman Parameters **********")
	       // //fmt.Printf("Randomly Generated Prime: %d\n", randomPrimeInt)
	       // //return randomPrimeInt
        // //       //return randomPrime
        //    }

    
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class theTeardrop : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public String[] possibleWords;

   public String[] condition1Names;
   public String[] condition2Names;
   public String[] condition3Names;
   public String[] condition4Names;
   public String[] otherwiseNames;

   public int[] personCryingInNumbers;
   public int[] correctWordInNumbers;

   public TextMesh displayTextMesh;

   public List <int> serialNumberNumbers = new List <int>();

   public KMSelectable Teardrop;
   
   public string personCrying;
   public string firstIndicator;
   public string correctWord;
   public string encryptedWord;

   public string originalAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
   public string correctString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

   public int sumOfSerialNumberDigits = 0;
   public int numberOfOnIndicators = 0;
   public int numberOfOffIndicators = 0;
   public int numberOfModules = 0;
   public int numberOfSerialNumberLetters = 0;
   public int matchesBetweenSerialNumberAndTEARDROPS = 0;
   public int oddIndexToMove = 0;
   public int evenIndexToMove = 0;
   public int finalAnswer = 0;
   public int sumOfLetterValuesOfPersonCrying = 0;
   public int sumOfLetterValuesOfDecryptedWord = 0;
   public int stage = 0;


   public int halfLength;
   public string secondHalf;
   public string firstHalf;

   public string serialNumber;

   private string withoutPrimeLetters;
   private string withoutCRYLetters;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   void Awake () { //Avoid doing calculations in here regarding edgework. Just use this for setting up buttons for simplicity.
      ModuleId = ModuleIdCounter++;
      Teardrop.OnInteract += delegate () { TeardropPressed(); return false; };
   }

   void Start () { //Shit that you calculate, usually a majority if not all of the module
      CorrectWordChooser();
      EncryptWord();
      GetFirstIndicator();
      DeterminePersonCrying();
      FinalAnswer();
   }

   void CorrectWordChooser(){ 
      int possibleWordsIndex = UnityEngine.Random.Range(0, 40);
      correctWord = possibleWords[possibleWordsIndex].ToUpper();
      Debug.Log("The decrypted word should be: " + correctWord);
   }

   bool CalcIsPrime(int number) {

    if (number == 1) return false;
    if (number == 2) return true;
    if (number % 2 == 0) return false; // Even number     

    for (int i = 2; i <= Math.Sqrt(number); i++) { // Check up to the square root
       if (number % i == 0) return false;
    }
    return true;
}
   void EncryptWord(){
      foreach (var a in Bomb.GetSerialNumberNumbers()){
         sumOfSerialNumberDigits += a;
      }

      Debug.Log("Starting string: " + originalAlphabet);
      if((Bomb.GetBatteryCount()*Bomb.GetPortCount()) % 2 == 1){
         correctString = "ZYXWVUTSRQPONMLKJIHGFEDCBA";
         Debug.Log("Since the product of the number of batteries and number of ports is odd, the string must be reversed.");
         Debug.Log("Current string: " + correctString);
      }

      Debug.Log(CalcIsPrime(sumOfSerialNumberDigits));
      if(CalcIsPrime(sumOfSerialNumberDigits)){
         withoutPrimeLetters = correctString.Replace("P", "").Replace("R", "").Replace("I", "").Replace("M", "").Replace("E", "");
         correctString = "PRIME" + withoutPrimeLetters;

         Debug.Log("Since the sum of the digits in the serial number is prime, the letters in the substring PRIME must be removed from the current string, then added as a whole at the beginning.");
         Debug.Log("Current string: " + correctString);
      }

         foreach (var letter in Bomb.GetSerialNumberLetters()){
            numberOfSerialNumberLetters++;
         }

      if(Bomb.GetBatteryCount()>numberOfSerialNumberLetters){
         halfLength = correctString.Length / 2;
         // Take the second half of the string
         secondHalf = correctString.Substring(halfLength);
         // Take the first half of the string
         firstHalf = correctString.Substring(0, halfLength);
         // Move the second half in front of the first half
         correctString = secondHalf + firstHalf;
         Debug.Log("Since the number of batteries is greater than the number of letters in the serial number, the second half of the string must be cut and moved to the front. ");
         Debug.Log("Current string: " + correctString);
      }

      serialNumber = Bomb.GetSerialNumber();

      foreach (char character in "TEARDROP")
         {
               if (serialNumber.Contains(character))
               {
                  matchesBetweenSerialNumberAndTEARDROPS++;
               }
         }

      if (matchesBetweenSerialNumberAndTEARDROPS >= 1) {
         withoutCRYLetters = correctString.Replace("C", "").Replace("R", "").Replace("Y", "");
         correctString = withoutCRYLetters + "CRY";
         Debug.Log("Since the Serial Number contains at least one character in the word TEARDROP, CRY must be extracted, then appended. ");
         Debug.Log("Current string: " + correctString);
      }

      foreach (var module in Bomb.GetModuleNames()){
         numberOfModules++;
      }

      if (numberOfModules % 2 == 0){
         evenIndexToMove = Bomb.GetSerialNumberNumbers().Last()-1;

         correctString = MoveCharacterToEnd(correctString, evenIndexToMove);

         Debug.Log("Since the total number of modules is even, the letter in the position of the last digit of the serial number must be extracted, then appended. ");
         Debug.Log("Current string: " + correctString);
      }

      if (numberOfModules % 2 == 1){
         oddIndexToMove = Bomb.GetSerialNumberNumbers().First()-1;

         correctString = MoveCharacterToEnd(correctString, oddIndexToMove);

         Debug.Log("Since the total number of modules is odd, the letter in the position of the first digit of the serial number must be extracted, then appended. ");
         Debug.Log("Current string: " + correctString);
      }

      if ((Bomb.GetPortCount(Port.Parallel) == 1) && (Bomb.IsIndicatorOn("BOB"))){
         correctString = originalAlphabet;
         Debug.Log("Since there is one parallel port and a lit BOB indicator is present, all previous rules no longer apply.");
         Debug.Log("Current string: " + correctString);
      }

      Debug.Log("Therefore, Final String:" + correctString);

      //========================================================================//

      int[] indicesArray = new int[correctWord.Length];
      
      foreach(char x in correctWord){
         indicesArray[stage] = correctString.IndexOf(x);
         encryptedWord += originalAlphabet[indicesArray[stage]];
         stage++;
      }

      Debug.Log("Thus, the encrypted word is " + encryptedWord);
      
      displayTextMesh.text = encryptedWord;

   }
//==================================================================================
   string MoveCharacterToEnd(string input, int index)
    {
        if (index >= 0 && index < input.Length - 1)
        {
            char charToMove = input[index];
            string modifiedString = input.Remove(index, 1);
            modifiedString += charToMove;
            return modifiedString;
        }

        return input;
    }
   
   static int[] ConvertLettersToAlphabetPositions(string input)
    {
        int[] result = new int[input.Length];
        int index = 0;

        foreach (char c in input)
        {
            if (char.IsLetter(c))
            {
                char upperC = char.ToUpper(c);
                int position = upperC - 'A' + 1;
                result[index++] = position;
            }
            else
            {
                // If the character is not a letter, store a placeholder value (e.g., 0)
                result[index++] = 0;
            }
        }

        // Resize the array to the actual number of letter positions
        Array.Resize(ref result, index);
        return result;
    }

   void GetFirstIndicator(){
      string[] sortedIndicators = Bomb.GetIndicators().OrderBy(s => s).ToArray();
      
      if ((sortedIndicators != null) && (sortedIndicators.Length > 0)){
         firstIndicator = sortedIndicators[0];
      } else {
         firstIndicator = "none";
         Debug.Log("There are no indicators.");
      }
   }

   void DeterminePersonCrying(){
      Debug.Log(sumOfSerialNumberDigits);

      foreach (var b in Bomb.GetOnIndicators()){
         numberOfOnIndicators += 1;
      }

      foreach (var b in Bomb.GetOffIndicators()){
         numberOfOffIndicators += 1;
      }

      if (Bomb.GetPortCount() % 2 == 0){
         Debug.Log("Since the number of ports is even, the first column should be used.");
         Debug.Log("The first indicator in alphabetical order is " + firstIndicator + ".");
         if (firstIndicator == "BOB") {
            personCrying = condition1Names[0];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "CAR") {
            personCrying = condition1Names[1];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "CLR") {
            personCrying = condition1Names[2];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "FRK") {
            personCrying = condition1Names[3];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "FRQ") {
            personCrying = condition1Names[4];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "IND") {
            personCrying = condition1Names[5];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "MSA") {
            personCrying = condition1Names[6];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "NSA") {
            personCrying = condition1Names[7];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "SIG") {
            personCrying = condition1Names[8];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "SND") {
            personCrying = condition1Names[9];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "TRN") {
            personCrying = condition1Names[10];
            Debug.Log("The person crying is " + personCrying +".");
         } else {
            personCrying = condition1Names[11];
            Debug.Log("The person crying is " + personCrying +".");
         }
      } else if (Bomb.GetBatteryCount()%2 == 1){
         Debug.Log("Since the number of batteries is odd, the second column should be used.");
         Debug.Log("The first indicator in alphabetical order is " + firstIndicator + ".");
         if (firstIndicator == "BOB") {
            personCrying = condition2Names[0];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "CAR") {
            personCrying = condition2Names[1];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "CLR") {
            personCrying = condition2Names[2];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "FRK") {
            personCrying = condition2Names[3];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "FRQ") {
            personCrying = condition2Names[4];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "IND") {
            personCrying = condition2Names[5];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "MSA") {
            personCrying = condition2Names[6];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "NSA") {
            personCrying = condition2Names[7];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "SIG") {
            personCrying = condition2Names[8];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "SND") {
            personCrying = condition2Names[9];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "TRN") {
            personCrying = condition2Names[10];
            Debug.Log("The person crying is " + personCrying +".");
         } else {
            personCrying = condition2Names[11];
            Debug.Log("The person crying is " + personCrying +".");
         }
      } else if (CalcIsPrime(sumOfSerialNumberDigits)){
         Debug.Log("Since the sum of the digits in the serial number is composite, the third column should be used.");
         Debug.Log("The first indicator in alphabetical order is " + firstIndicator + ".");
         if (firstIndicator == "BOB") {
            personCrying = condition3Names[0];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "CAR") {
            personCrying = condition3Names[1];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "CLR") {
            personCrying = condition3Names[2];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "FRK") {
            personCrying = condition3Names[3];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "FRQ") {
            personCrying = condition3Names[4];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "IND") {
            personCrying = condition3Names[5];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "MSA") {
            personCrying = condition3Names[6];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "NSA") {
            personCrying = condition3Names[7];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "SIG") {
            personCrying = condition3Names[8];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "SND") {
            personCrying = condition3Names[9];
            Debug.Log("The person crying is " + personCrying +".");
         } else if (firstIndicator == "TRN") {
            personCrying = condition3Names[10];
            Debug.Log("The person crying is " + personCrying +".");
         } else {
            personCrying = condition3Names[11];
            Debug.Log("The person crying is " + personCrying +".");
         } 
      } else if (numberOfOnIndicators > numberOfOffIndicators){
         Debug.Log("Since the number of unlit indicators is greater than the number of unlit indicators, the fourth column should be used.");
         Debug.Log("The first indicator in alphabetical order is " + firstIndicator + ".");
         if (firstIndicator == "BOB") {
            personCrying = condition4Names[0];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "CAR") {
            personCrying = condition4Names[1];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "CLR") {
            personCrying = condition4Names[2];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "FRK") {
            personCrying = condition4Names[3];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "FRQ") {
            personCrying = condition4Names[4];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "IND") {
            personCrying = condition4Names[5];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "MSA") {
            personCrying = condition4Names[6];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "NSA") {
            personCrying = condition4Names[7];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "SIG") {
            personCrying = condition4Names[8];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "SND") {
            personCrying = condition4Names[9];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "TRN") {
            personCrying = condition4Names[10];
            Debug.Log("The person crying is " + personCrying + ".");
         } else {
            personCrying = condition4Names[11];
            Debug.Log("The person crying is " + personCrying + ".");
         }
      } else {
         Debug.Log("Since none of the previous conditions apply, the last column should be used.");
         Debug.Log("The first indicator in alphabetical order is " + firstIndicator + ".");
         if (firstIndicator == "BOB") {
            personCrying = otherwiseNames[0];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "CAR") {
            personCrying = otherwiseNames[1];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "CLR") {
            personCrying = otherwiseNames[2];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "FRK") {
            personCrying = otherwiseNames[3];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "FRQ") {
            personCrying = otherwiseNames[4];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "IND") {
            personCrying = otherwiseNames[5];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "MSA") {
            personCrying = otherwiseNames[6];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "NSA") {
            personCrying = otherwiseNames[7];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "SIG") {
            personCrying = otherwiseNames[8];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "SND") {
            personCrying = otherwiseNames[9];
            Debug.Log("The person crying is " + personCrying + ".");
         } else if (firstIndicator == "TRN") {
            personCrying = otherwiseNames[10];
            Debug.Log("The person crying is " + personCrying + ".");
         } else {
            personCrying = otherwiseNames[11];
            Debug.Log("The person crying is " + personCrying + ".");
         }
      }

      
   }

   void FinalAnswer(){
      personCryingInNumbers = ConvertLettersToAlphabetPositions(personCrying);
      for(int b = 0; b < personCryingInNumbers.Length; b++){
         sumOfLetterValuesOfPersonCrying += personCryingInNumbers[b];
      }

      Debug.Log(sumOfLetterValuesOfPersonCrying);

      correctWordInNumbers = ConvertLettersToAlphabetPositions(correctWord);
      for(int c = 0; c < correctWordInNumbers.Length; c++){
         sumOfLetterValuesOfDecryptedWord += correctWordInNumbers[c];
      }
      Debug.Log(sumOfLetterValuesOfDecryptedWord);
      finalAnswer = (sumOfLetterValuesOfPersonCrying + sumOfLetterValuesOfDecryptedWord)%10;
      Debug.Log(finalAnswer + " is the correct last digit to press the button on.");
   }

   void TeardropPressed() {

      Teardrop.AddInteractionPunch();
      if (ModuleSolved){
         return;
      }
      string time = Bomb.GetFormattedTime();
      Debug.Log(time + " is the current time.");
      char lastDigitChar = time[time.Length - 1];
      int lastDigit = int.Parse(lastDigitChar.ToString());
         if (lastDigit == finalAnswer){
            Solve();
            Debug.Log("You pressed the button when the timer ended in " + finalAnswer + "! Well done!");
         } else {
            Strike();
            Debug.Log("The last digit of the timer was " + lastDigit + ". Expected last digit was " + finalAnswer + ". Please try again.");
         }
   }


   void Solve () {
      Audio.PlaySoundAtTransform("TEARDROP SOLVED SOUND", transform);
      GetComponent<KMBombModule>().HandlePass();
   }

   void Strike () {
      GetComponent<KMBombModule>().HandleStrike();
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      yield return null;
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return null;
   }
}

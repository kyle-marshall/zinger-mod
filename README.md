# zinger-mod

This mod adds a Zinger. It's like the Singer, and even found in the same component category.
Unlike the Singer it it quite wide and has 9 input pins. 8 of these are used to select the note, and it can even hit some in-between notes (see below).

## THE DATA SHEET

Pins: E D0 D1 D2 D3 D4 D5 D6 D7
- E: The lonely pin to the side is the enable pin. Activating this pin makes the zinger zing.
- DX: The rest of the pins shift the note up by a certain number of note intervals or "steps". Starting from the pin closest to enable...
  D0: 2**-2 = 0.25 steps
  D1: 2**-1 = 0.5 steps
  D2: 2**0 = 1 step
  D3: 2**1 = 2 steps
  ...
  D7: 2**5 = 32 steps

You can configure the base note by choosing a note in the Edit Component dialog ('X').
The final note is base note + steps as determined by the inputs.
This final note is displayed on the side of the Zinger while it is not off.

# Issues
- No issues for me so far. I thought there was a memory leak at one point but it turned out leaving `loglevel "trace"` on just generates extreme amounts of spam in the game console.
- Let me know if you have any issues or suggestions!

# Videos

- the earliest documented zinger prototype: https://www.youtube.com/watch?v=MN-WloAfhZ0
- more sounds from the zinger: https://www.youtube.com/watch?v=Uv1JgwYEVWM

Keep on zinging

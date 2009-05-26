using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusTetris.Core
{
    /*
    A - Addline block, adds one line to a field. 
C - Clear line block, clears one line of a field. 
N - Nuke block, clears an entire field of blocks, use on allies or yourself. 
R - Random blocks clear, clears ten random blocks from a field. 
S - Switch block, switches your field with another player's. 
B - Clear special blocks, turns all special blocks in a field to ordinary blocks. 
G - Gravity block, gets rid of all holes in a field and sucks all blocks downward, getting rid of special blocks in the process. 
Q - Block quake, shifts the blocks of a field all around, destroying possible tetri. (plural for tetris, hehe) 
O - Block bomb, blows a 3x3 hole around all O blocks and relocates the blocks, usually near the top of the field. 

     * */
    /// <summary>
    /// les options du jeu :
    /// - a : add line
    /// - b : clear all option in board, clear special block
    /// - c : clear line
    /// - e : erase board
    /// - g : gravity
    /// - h : Hide my board to anothers during 10 secondes
    /// - l : gravity left
    /// - n : nuke field
    /// - o : block bomb, blows a hole around all 'o' blocks 
    /// - q : quake, shift blocks of board
    /// - r : gravity right
    /// - s : switch board with another
    /// </summary>
    static class Options
    {
        public static byte[] Available = new byte[] { 
            (byte)'a', 
            (byte)'b', 
            (byte)'c', 
            //(byte)'e',
            (byte)'g',
            (byte)'h',
            (byte)'l', 
            (byte)'n', 
            (byte)'o', 
            (byte)'q', 
            (byte)'r', 
            (byte)'s' 
        };
    }
}

/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package mapimagetool;

/**
 *
 * @author xheno
 */
public class DisplayInfo {
    public int PixelDensity;
    public int MapSqX ;
    public int MapSqY;
    public int MinResX;
    public int MinResY;
    public DisplayInfo()
    {
        PixelDensity = 100;
        MapSqX = 18;
        MapSqY = 12;
        MinResX = 3600;
        MinResY = 2400;
    }
}

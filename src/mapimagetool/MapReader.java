/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package mapimagetool;

import javax.swing.*;
import java.io.*;
import java.nio.file.*;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Scanner;
import org.json.simple.*;

/**
 *
 * @author xheno
 */
public class MapReader {

    public Path StartDirectory;
    public Path DepoDirectory;
    public DisplayInfo Display;
    public JSONArray MapArray;
    Scanner scanner;

    public MapReader() {
        scanner = new Scanner(System.in);
        StartDirectory = Paths.get(System.getProperty("user.dir"), "Map Depos");
        File startDepo = new File(StartDirectory.toString());
        startDepo.mkdirs();
        DepoDirectory = null;
        ArrayList<File> depoList = new ArrayList<File>(Arrays.asList(startDepo.listFiles()));
        if (depoList.size() < 1) {
            System.out.printf("No map depo folders found in base directory.%nCreate new one? (Y or N): ");
            switch (scanner.nextLine().toUpperCase()) {
                case "Y":
                    CreateDepo();
                    break;
                default:
                    break;

            }
        } else {
            System.out.printf("%nSelect Map Folder:%n");
            int i;
            for (i = 0; i < depoList.size(); i++) {
                System.out.printf("%d: %s%n", i + 1, depoList.get(i).getName());
            }
            System.out.printf("%d: %s%n", i + 1, "Create new depo");
            int depoSelect = 0;
            String input = scanner.nextLine();
            if (isInteger(input)) {
                depoSelect = Integer.parseInt(input);
            } else {
                System.out.println("Not a valid selection.");
                return;
            }

            if (depoSelect < 1 || depoSelect > depoList.size() + 1) {
                System.out.println("Not a valid selection.");
                return;
            } else if (depoSelect == i + 1) {
                CreateDepo();
            } else {
                DepoDirectory = Paths.get(depoList.get(i-1).getAbsolutePath());
            }

            System.out.printf("Selecting %s%n", DepoDirectory.toString());
            //SetDepoInfo()
            //OpenMainMenu()

        }

    }

    public void CreateDepo() {
        System.out.print("New map depo name: ");
        String mapDepoName = scanner.nextLine();
        File newDepoFile = new File(Paths.get(StartDirectory.toString(), mapDepoName).toString());
        DepoDirectory = Paths.get(newDepoFile.toString());
        newDepoFile.mkdirs();
        new File(Paths.get(DepoDirectory.toString(), "Source Images").toString()).mkdir();
        new File(Paths.get(DepoDirectory.toString(), "Resized Maps").toString()).mkdir();
        new File(Paths.get(DepoDirectory.toString(), "Tiled Maps").toString()).mkdir();
        
    }

    public void OpenMainMenu() {

    }

    public boolean SetDepoInfo() { 
        if (DepoDirectory == null) {
            System.out.println("ERROR: Map Folder directory has not been set.");
            return false;
        } else {
            File infoFile = new File(Paths.get(DepoDirectory.toString(), "DisplayInfo.JSON").toString());
            if (infoFile.exists()) {
                // set DisplayInfo to base type
            } else {
                // set JSON object to deserialized file
            }
        }
        return true;
    }
    
    public void AddMapJSON() {
        
    }
    
    // public String ReadFilesInDirectory(String path) { } 
    
    public void AddResizedMap() {
        
    }
    
    public void ResizeMap() {
        
    }
    
    public void SetMapFill() {
        
    }
    
    public void AddTiledMap() {
        // needs to be updated from original file
    }
    
    public void TileMap() {
        // needs to be updated from original file
    }
    
    public static boolean isInteger(String str) {
        int length = str.length();
        if (str == null) {
            return false;
        }
        if (str.isEmpty()) {
            return false;
        }
        int i = 0;
        if (str.charAt(0) == '-') {
            if (length == 1) {
                return false;
            }
            i = 1;
        }
        for (; i < length; i++) {
            char c = str.charAt(i);
            if (c < '0' || c > '9') {
                return false;
            }
        }
        return true;
    }

}

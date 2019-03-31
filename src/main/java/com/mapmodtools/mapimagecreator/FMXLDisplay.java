/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.mapmodtools.mapimagecreator;

import javafx.application.Application;
import javafx.event.ActionEvent;
import javafx.event.EventHandler;
import java.net.*;
import javafx.fxml.*;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.control.*;
import javafx.geometry.*;
import javafx.stage.Stage;
import javafx.scene.layout.*;
import javafx.scene.layout.StackPane;
/**
 *
 * @author xheno
 */
public class FMXLDisplay extends Application {
    public static void main(String[] args) {
        //ButtonExample ex = new ButtonExample();
        //MapReader reader = new MapReader();
        //test
        launch(args);
        
        System.exit(0);
    }
    
    @Override
    public void start(Stage stage) throws Exception {
       Parent root = FXMLLoader.load(getClass().getResource("fxml_example.fxml"));
    
        Scene scene = new Scene(root, 300, 275);
    
        stage.setTitle("FXML Welcome");
        stage.setScene(scene);
        stage.show();
    }
}

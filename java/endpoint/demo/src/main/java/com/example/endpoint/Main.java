package com.example.endpoint;

import java.io.IOException;
import java.io.OutputStream;
import java.net.InetSocketAddress;
import java.nio.charset.StandardCharsets;

import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpServer;

public class Main {
    public static void main(String[] args) throws IOException {
        // System.out.println("Hello world!");

        HttpServer server = HttpServer.create(new InetSocketAddress(8081), 0);

        server.createContext("/hello", Main::handleHelloRequest);
        server.setExecutor(null);

        server.start();
    }

    private static void handleHelloRequest(HttpExchange exchange) throws IOException {
        String response = "Hello world!";
        byte[] responseBytes = response.getBytes(StandardCharsets.UTF_8);
        exchange.sendResponseHeaders(200, responseBytes.length);
        
        try (OutputStream os = exchange.getResponseBody()) {
            os.write(responseBytes);
        }
    }

}
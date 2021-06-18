use std::env;

#[tokio::main]
async fn main() {
    let port_string = 
        env::var("SERVER_PORT")
        .or::<String>(Ok("".to_string()))
        .unwrap();

    let port = 
        port_string.parse::<i16>()
        .or::<i16>(Ok(9865))
        .unwrap();

    

    println!("port: {}", port);
}
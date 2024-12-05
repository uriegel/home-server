use std::{thread, error::Error};

use home_server::{http_server::start_http_server, config::Config};
use home_server::https_server::start_https_server;
use signal_hook::{iterator::Signals, consts::{SIGINT, SIGTERM}};
use tokio::runtime::Runtime;

// TODO update hyper, but then there are errors 

fn main() -> Result<(), Box<dyn Error>> {
    println!("starting home server...");

    let config = Config::new()?;
    let rt = Runtime::new()?;
    let config_https = config.clone();
    start_http_server(&rt, config);
    if config_https.tls_port > 0 { start_https_server(&rt, config_https); }

    println!("Home server started");

    let mut signals = Signals::new(&[SIGINT, SIGTERM])?;
    let shutdown_listener = thread::spawn(move || {
        for sig in signals.forever() {
            println!("Received signal {sig:?}");
            break;
        }
    });    
    shutdown_listener.join().unwrap();

    println!("Stopping home server...");
    println!("Home server stopped");
    Ok(())
}






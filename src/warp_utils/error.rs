#[derive(Debug)]
pub struct Error {}

impl Error {
    pub fn from_io(_e: std::io::Error) -> warp::Rejection{
        warp::reject::custom(Error{})
    }
}

pub async fn return_error(e: warp::Rejection)->Result<impl warp::Reply, warp::Rejection> {
    println!("Error: {:?}", e);

    if let Some(e) = e.find::<Error>() {
        println!("E-Error: {:?}", e);
    }

    Ok(warp::http::Response::builder()
        .status(404)
        .body(hyper::Body::empty())
        .unwrap())
}

impl From<std::io::Error> for Error {
    fn from(_error: std::io::Error) -> Self {
        Error {}
    }
}

impl warp::reject::Reject for Error {}


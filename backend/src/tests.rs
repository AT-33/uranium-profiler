use crate::*;
use std::time::Duration;

#[test]
fn test_add_event() {
    initialize_profiler();
    unsafe { EVENTS.clear(); }
    let test_name = "test_name";
    let time_sleep = 1000i128;

    profiler_event(test_name, EventType::Begin);
    std::thread::sleep(Duration::from_millis(time_sleep as u64));
    profiler_event(test_name, EventType::End);

    let begin = unsafe { EVENTS[0] };
    let end = unsafe { EVENTS[1] };
    let delta_time = (end.0 - begin.0) as i128;

    assert_eq!(begin.1, test_name);
    assert_eq!(begin.2, EventType::Begin);

    assert_eq!(end.1, test_name);
    assert_eq!(end.2, EventType::End);

    assert!((delta_time - time_sleep * 1000).abs() < 20000);
}

fn factorial(x: f64) -> f64 {
    profiler_event("test_name", EventType::Begin);

    let f = match x {
        0.0..=1.0 => 1.0,
        _ => x * factorial(x - 1),
    };
    profiler_event("test_name", EventType::End);
    f
}

#[test]
fn test_save_session() {
    initialize_profiler();
    unsafe { EVENTS.clear(); }

    factorial(10.0);

    save_current_session();
}

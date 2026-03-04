use rapier3d::na::ComplexField;

#[unsafe(no_mangle)]
extern "C" fn acos(x: f32) -> f32 {
    ComplexField::acos(x)
}

#[unsafe(no_mangle)]
extern "C" fn acosh(x: f32) -> f32 {
    ComplexField::acosh(x)
}

#[unsafe(no_mangle)]
extern "C" fn asin(x: f32) -> f32 {
    ComplexField::asin(x)
}

#[unsafe(no_mangle)]
extern "C" fn asinh(x: f32) -> f32 {
    ComplexField::asinh(x)
}

#[unsafe(no_mangle)]
extern "C" fn atan(x: f32) -> f32 {
    ComplexField::atan(x)
}

#[unsafe(no_mangle)]
extern "C" fn atanh(x: f32) -> f32 {
    ComplexField::atanh(x)
}

#[unsafe(no_mangle)]
extern "C" fn cos(x: f32) -> f32 {
    ComplexField::cos(x)
}

#[unsafe(no_mangle)]
extern "C" fn cosh(x: f32) -> f32 {
    ComplexField::cosh(x)
}

#[unsafe(no_mangle)]
extern "C" fn log(x: f32) -> f32 {
    ComplexField::ln(x)
}

#[unsafe(no_mangle)]
extern "C" fn sin(x: f32) -> f32 {
    ComplexField::sin(x)
}

#[unsafe(no_mangle)]
extern "C" fn sinh(x: f32) -> f32 {
    ComplexField::sinh(x)
}

#[unsafe(no_mangle)]
extern "C" fn tan(x: f32) -> f32 {
    ComplexField::tan(x)
}

#[unsafe(no_mangle)]
extern "C" fn tanh(x: f32) -> f32 {
    ComplexField::tanh(x)
}

#[unsafe(no_mangle)]
extern "C" fn sqrt(x: f32) -> f32 {
    if let Some(result) = ComplexField::try_sqrt(x){
        return result;
    }
    f32::NAN
}
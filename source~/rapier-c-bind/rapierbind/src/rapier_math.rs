use rapier3d::na::ComplexField;

#[unsafe(no_mangle)]
extern "C" fn cf_acos(x: f32) -> f32 {
    ComplexField::acos(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_acosh(x: f32) -> f32 {
    ComplexField::acosh(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_asin(x: f32) -> f32 {
    ComplexField::asin(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_asinh(x: f32) -> f32 {
    ComplexField::asinh(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_atan(x: f32) -> f32 {
    ComplexField::atan(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_atanh(x: f32) -> f32 {
    ComplexField::atanh(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_cos(x: f32) -> f32 {
    ComplexField::cos(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_cosh(x: f32) -> f32 {
    ComplexField::cosh(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_log(x: f32) -> f32 {
    ComplexField::ln(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_sin(x: f32) -> f32 {
    ComplexField::sin(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_sinh(x: f32) -> f32 {
    ComplexField::sinh(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_tan(x: f32) -> f32 {
    ComplexField::tan(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_tanh(x: f32) -> f32 {
    ComplexField::tanh(x)
}

#[unsafe(no_mangle)]
extern "C" fn cf_sqrt(x: f32) -> f32 {
    if let Some(result) = ComplexField::try_sqrt(x){
        return result;
    }
    f32::NAN
}
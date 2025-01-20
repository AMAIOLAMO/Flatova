project(
  'flatova', 'cpp',
  default_options: 'default_library=static',
  version: '0.0.1'
)

srcs = files(
  'flatova.cpp'
)

subdir('private')

glfw3deps = [dependency('glfw3'), dependency('gl')]
imguidep = dependency('imgui')
vulkandep = dependency('vulkan')

public_inc = include_directories('public')

exe = executable('flatova',
  sources: srcs,
  win_subsystem: 'windows',
  dependencies: [glfw3deps, imguidep, vulkandep],
  include_directories: public_inc
)

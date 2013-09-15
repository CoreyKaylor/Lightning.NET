require 'rake/clean'
require 'rbconfig'
include RbConfig

CLEAN.include("**/*.o", "**/bin/**/*.{dylib,dll,dbg,mdb,so}")

@cc = "cc"
LMDB_DIR = "mdb/libraries/liblmdb"
BIN_DIR = "src/LightningDB.Tests/bin/Debug"

begin
  require 'bundler/setup'
  require 'fuburake'
rescue LoadError
  puts 'Bundler and all the gems need to be installed prior to running this rake script. Installing...'
  system("gem install bundler --source http://rubygems.org")
  sh 'bundle install'
  system("bundle exec rake", *ARGV)
  exit 0
end


@solution = FubuRake::Solution.new do |sln|
	sln.assembly_info = {
		:product_name => "Lightning.NET",
		:copyright => 'Copyright 2013 Ilya Lukyanov.',
		:description => '.NET-wrapper library for OpenLDAP\'s Lightning DB key-value store.'
	}

	sln.ripple_enabled = true
  sln.precompile = [:native_compile_32, :native_compile_64]
end

directory BIN_DIR

def add_tasks(bits)

  file "#{LMDB_DIR}/mdb#{bits}.o" => ["#{LMDB_DIR}/mdb.c"] do |t|
    sh "#{@cc} -pthread -O2 -g -W -Wno-unused-parameter -Wbad-function-cast -fPIC -c -o #{t.name} #{t.prerequisites.first}"
  end

  file "#{LMDB_DIR}/midl#{bits}.o" => ["#{LMDB_DIR}/midl.c"] do |t|
    sh "#{@cc} -pthread -O2 -g -W -Wno-unused-parameter -Wbad-function-cast -fPIC -c -o #{t.name} #{t.prerequisites.first}"
  end

  file "#{BIN_DIR}/liblmdb#{bits}.dylib" => ["#{LMDB_DIR}/mdb#{bits}.o", "#{LMDB_DIR}/midl#{bits}.o"] do |t|
    sh "#{@cc} -shared -o #{t.name} #{t.prerequisites.join(' ')}"
  end

  file "#{BIN_DIR}/lmdb#{bits}.dll" => ["#{LMDB_DIR}/mdb#{bits}.o", "#{LMDB_DIR}/midl#{bits}.o"] do |t|
    sh "#{@cc} -shared -o #{t.name} #{t.prerequisites.join(' ')}"
  end

  file "#{BIN_DIR}/liblmdb#{bits}.so" => ["#{LMDB_DIR}/mdb#{bits}.o", "#{LMDB_DIR}/midl#{bits}.o"] do |t|
    sh "#{@cc} -shared -o #{t.name} #{t.prerequisites.join(' ')}"
  end

  task "native_compile_#{bits}" => [BIN_DIR] do
    native_compile(bits)
  end
end

add_tasks(32)
add_tasks(64)

def native_compile(bits)
  host = RbConfig::CONFIG['host_os']
  puts "Native compiling #{host}"

  @cc = "cc -m32" if bits == 32
  @cc = "cc -m64" if bits == 64

  case host
    when /mswin|mingw/i
      @cc = "i686-w64-mingw32-gcc" if bits == 32
      @cc = "x86_64-w64-mingw32-gcc" if bits == 64

      Rake::Task["#{BIN_DIR}/lmdb#{bits}.dll"].invoke
    when /linux/i
      Rake::Task["#{BIN_DIR}/liblmdb#{bits}.so"].invoke
    when /darwin/i
      Rake::Task["#{BIN_DIR}/liblmdb#{bits}.dylib"].invoke
  end
end

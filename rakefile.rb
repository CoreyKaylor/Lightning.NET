require 'rake/clean'
require 'rbconfig'
include RbConfig

CLEAN.include("**/*.o", "**/bin/**/*.{dylib,dll,dbg,mdb,so}")

LMDB_DIR = "mdb/libraries/liblmdb"
BIN_DIR = "src/LightningDB.Tests/bin/Debug"
@cc = "cc"

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
  sln.precompile = [:native_compile]
end

rule ".o" => ".c" do |t|
  sh "#{@cc} -pthread -O2 -g -W -Wno-unused-parameter -Wbad-function-cast -fPIC -c -o #{t.name} #{t.source}"
end

file "#{LMDB_DIR}/mdb.o" => ["#{LMDB_DIR}/mdb.c", "#{LMDB_DIR}/lmdb.h", "#{LMDB_DIR}/midl.h"]

file "#{LMDB_DIR}/midl.o" => ["#{LMDB_DIR}/midl.c", "#{LMDB_DIR}/midl.h"]

file "#{BIN_DIR}/liblmdb.dylib" => ["#{LMDB_DIR}/mdb.o", "#{LMDB_DIR}/midl.o"] do |t|
  sh "#{@cc} -shared -o #{t.name} #{t.prerequisites.join(' ')}"
end

file "#{BIN_DIR}/lmdb.dll" => ["#{LMDB_DIR}/mdb.o", "#{LMDB_DIR}/midl.o"] do |t|
  sh "#{@cc} -shared -o #{t.name} #{t.prerequisites.join(' ')}"
end

file "#{BIN_DIR}/liblmdb.so" => ["#{LMDB_DIR}/mdb.o", "#{LMDB_DIR}/midl.o"] do |t|
  sh "#{@cc} -shared -o #{t.name} #{t.prerequisites.join(' ')}"
end

task :native_compile do
  host = RbConfig::CONFIG['host_os']
  puts "Native compiling #{host}"
  case host
    when /mswin|mingw/i
      @cc = "gcc"
      Rake::Task["#{BIN_DIR}/lmdb.dll"].invoke
    when /linux/i
      Rake::Task["#{BIN_DIR}/liblmdb.so"].invoke
    when /darwin/i
      @cc = "cc -m32"
      Rake::Task["#{BIN_DIR}/liblmdb.dylib"].invoke
  end
end
